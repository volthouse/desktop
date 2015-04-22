#pragma once

#include <vector>
#include "Resistors.h"

class CCalculationCallback {
public:
	virtual void OnAsyncCalculationEnded() { };
};

// CCalculationThread

class CCalculationThread : public CWinThread
{

	DECLARE_DYNCREATE(CCalculationThread)

protected:
	CCalculationThread();           // protected constructor used by dynamic creation
	virtual ~CCalculationThread();
	

public:
	bool m_canceled;
	float m_resistance;
	ResistorCircuitry m_curcuitry;
	std::vector<CResistor>* m_resistors;
	std::vector<CResistor> m_result;	
	CCalculationCallback* m_parent;	

	virtual BOOL InitInstance();
	virtual int ExitInstance();

protected:
	DECLARE_MESSAGE_MAP()
public:
	void Cancel();
	void SetData(float resistance, ResistorCircuitry circuitry, std::vector<CResistor>* resistors);
	virtual int Run();
};


