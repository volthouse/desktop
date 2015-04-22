#pragma once
#include "afxcmn.h"
#include "ResistorListView.h"
#include "afxwin.h"

// CResistorsCtrlPage dialog

class CResistorsCtrlPage : public CPropertyPage
{
	DECLARE_DYNAMIC(CResistorsCtrlPage)

public:
	CResistorsCtrlPage();
	virtual ~CResistorsCtrlPage();

// Dialog Data
	enum { IDD = IDD_RESISTORS_CONTROL };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	DECLARE_MESSAGE_MAP()
public:
	CResistorListView m_listView;

	virtual BOOL OnInitDialog();
	CComboBox m_setCombo;
	afx_msg void OnCbnSelchangeResistorspageResistorsCombo1();
	void SetResistorSet(int id);
	void InitializeResistorSetCombo(void);
};
