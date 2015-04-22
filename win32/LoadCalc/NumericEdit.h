// CNumericEdit

#pragma once
#include <afx.h>
#include <afxwin.h>

//#include "editex.h"



class CNumericEdit : public CEdit
{
	DECLARE_DYNAMIC(CNumericEdit)

public:
	CNumericEdit();
	virtual ~CNumericEdit();

protected:
	DECLARE_MESSAGE_MAP()
public:	
	static bool IsNumberLPC(LPCTSTR lpszText);
	afx_msg void OnChar(UINT nChar, UINT nRepCnt, UINT nFlags);
	FLOAT GetFloatValue();
	afx_msg void OnDropFiles(HDROP hDropInfo);
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
};


