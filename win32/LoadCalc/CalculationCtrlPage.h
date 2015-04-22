#pragma once
#include "afxcmn.h"
#include "afxwin.h"
#include "Resistors.h"
#include "NumericEdit.h"
#include "CalculationListView.h"
#include "CalculationThread.h"
#include <vector>

#define WM_ONCALCULATIONENDED (WM_USER + 110)

class CCalculationCtrlPage : public CPropertyPage, public CCalculationCallback
{
	DECLARE_DYNAMIC(CCalculationCtrlPage)

public:
	CCalculationCtrlPage();
	virtual ~CCalculationCtrlPage();

// Dialog Data
	enum { IDD = IDD_CALCULATION_CONTROL };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	DECLARE_MESSAGE_MAP()

public:	
	std::vector<CCalculationViewItem> m_viewItems;
	int m_currentItemIdx;
	CCalculationListView m_resultListView;
	CNumericEdit m_searchValueEdit;
	CNumericEdit m_viValueEdit;
	CEdit m_powerValuesEdit;
	CButton m_voltageRadio;
	CButton m_currentRadio;
	CComboBox m_circuitryCombo;
	CStatusBar m_bar;
	CCalculationThread* m_calcThread; 

	void CalculateParallelPower(int itemIdx);
	void CalculateSeriesPower(int itemIdx);
	void CalcPower(int itemIdx);	
	virtual void OnAsyncCalculationEnded();	
	virtual BOOL OnInitDialog();

	afx_msg void OnBnClickedCalculationPageCalcBtn();
	afx_msg LRESULT OnItemClicked(WPARAM wParam, LPARAM lParam);	
	afx_msg void OnBnClickedCalculationPageVoltageRadio();
	afx_msg void OnBnClickedCalculationPageCurrentRadio();
	afx_msg void OnEnChangeCalculationPageVivalueEdit();
	afx_msg void OnCbnSelchangeCalculationpageModeCombo();
	afx_msg LRESULT OnCalculationEnded(WPARAM wParam, LPARAM lParam);
};
