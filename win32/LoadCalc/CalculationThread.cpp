// CalculationThread.cpp : implementation file
//

#include "stdafx.h"
#include "LoadCalc.h"
#include "CalculationThread.h"


// CCalculationThread

IMPLEMENT_DYNCREATE(CCalculationThread, CWinThread)

CCalculationThread::CCalculationThread():m_canceled(false)
{
}

CCalculationThread::~CCalculationThread()
{
}

BOOL CCalculationThread::InitInstance()
{
	// TODO:  perform and per-thread initialization here
	return TRUE;
}

int CCalculationThread::ExitInstance()
{
	// TODO:  perform any per-thread cleanup here
	return CWinThread::ExitInstance();
}

BEGIN_MESSAGE_MAP(CCalculationThread, CWinThread)
END_MESSAGE_MAP()


// CCalculationThread message handlers

void CCalculationThread::SetData(float resistance, ResistorCircuitry circuitry, std::vector<CResistor>* resistors)
{
	m_resistance = resistance;
	m_curcuitry = circuitry;
	m_resistors = resistors;
}

int CCalculationThread::Run()
{
	CCircuitCalculator calculator;
	calculator.FindLoad(m_resistance, m_curcuitry, m_resistors, &m_result);
	
	if(!m_canceled) {
		m_parent->OnAsyncCalculationEnded();
	}
	
	return CWinThread::Run();
}

void CCalculationThread::Cancel()  { 
	m_canceled = true; 
	delete this;
};