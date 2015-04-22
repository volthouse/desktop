// CmnCtrl3.h : main header file for the CmnCtrl3 application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols
#include <vector>
#include "Resistors.h"
#include <shlobj.h>

// CLoadCalc:
// See CmnCtrl3.cpp for the implementation of this class
//

class CLoadCalc : public CWinApp
{
public:
	CLoadCalc();

	std::vector<CResistorSet> m_resistorSets;
	int m_currentSet;
	ResistorCircuitry m_circuitry;

	std::vector<CResistor>* GetCurrentSet() {
		return &m_resistorSets.at(m_currentSet).m_resistors;
	}

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CLoadCalc theApp;
