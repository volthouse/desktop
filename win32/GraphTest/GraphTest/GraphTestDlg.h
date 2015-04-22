
// GraphTestDlg.h : header file
//

#pragma once

#include "Graph.h"
#include "Signal.h"

// CGraphTestDlg dialog
class CGraphTestDlg : public CDialog
{
// Construction
public:
	CGraphTestDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_GRAPHTEST_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	CGraph m_Graph;

	CSignal m_Signal;
	CCosine m_CosSignal;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
};
