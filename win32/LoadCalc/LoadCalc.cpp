// CmnCtrl3.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"

#include "LoadCalc.h"
#include "AllControlsSheet.h"
#include "Resistors.h"
#include <locale.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// CLoadCalc

BEGIN_MESSAGE_MAP(CLoadCalc, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()

// CLoadCalc construction
CLoadCalc::CLoadCalc() : m_circuitry(Parallel)
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

// The one and only CLoadCalc object
CLoadCalc theApp;

// CLoadCalc initialization
BOOL CLoadCalc::InitInstance()
{
	AfxEnableControlContainer();

	// System-default ANSI code page
	setlocale(LC_ALL, "");

	// Read Resistor Values
	CString sPath = GetCommandLine();
    sPath = sPath.Mid(1, sPath.ReverseFind('\\') - 1);
	sPath.Append(L"\\LoadCalc.INI");

	CResistorImport import;	

	for(int i = 1; i < 10; ++i) {
		TCHAR sResistors[4096];
		CString sItem(L"Set");
		sItem.AppendFormat(L"%d", i);
		GetPrivateProfileString(L"Resistors", sItem, L"", sResistors, _countof(sResistors), sPath);
		if(wcslen(sResistors) > 0) {
			CResistorSet set;
			set.Name = sItem;
			import.ParseString(sResistors, &set.m_resistors);
			m_resistorSets.push_back(set);
		}
	}

	// Standard initialization

	CAllControlsSheet   allcontrolssheet(_T("Resistor Load Calculator"));
	m_pMainWnd = &allcontrolssheet;
	allcontrolssheet.DoModal();
	return FALSE;
}
