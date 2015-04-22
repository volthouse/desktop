// alculateThread.cpp : implementation file
//

#include "stdafx.h"
#include "LoadCalc.h"
#include "alculateThread.h"


// CalculateThread

IMPLEMENT_DYNCREATE(CalculateThread, CWinThread)

CalculateThread::CalculateThread()
{
}

CalculateThread::~CalculateThread()
{
}

BOOL CalculateThread::InitInstance()
{
	// TODO:  perform and per-thread initialization here
	return TRUE;
}

int CalculateThread::ExitInstance()
{
	// TODO:  perform any per-thread cleanup here
	return CWinThread::ExitInstance();
}

BEGIN_MESSAGE_MAP(CalculateThread, CWinThread)
END_MESSAGE_MAP()


// CalculateThread message handlers
