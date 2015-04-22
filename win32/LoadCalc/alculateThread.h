#pragma once



// CalculateThread

class CalculateThread : public CWinThread
{
	DECLARE_DYNCREATE(CalculateThread)

protected:
	CalculateThread();           // protected constructor used by dynamic creation
	virtual ~CalculateThread();

public:
	virtual BOOL InitInstance();
	virtual int ExitInstance();

protected:
	DECLARE_MESSAGE_MAP()
};


