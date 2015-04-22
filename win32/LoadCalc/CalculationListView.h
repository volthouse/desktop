#pragma once

#include "Resistors.h"
#include <vector>

// CCalculationListView

#define WM_ONITEMCLICKEDMESSAGE (WM_USER + 100)

class CCalculationViewItem {
public:
	CCalculationViewItem(float nominal, CResistor r):Nominal(nominal), Resistor(r) 
	{
		Resistor = r;
		Deviation = 1 - (Resistor.Value / Nominal);
	}

	CResistor Resistor;
	float Nominal;
	float Deviation;

	bool operator<(CCalculationViewItem item) { 
		return fabs(Deviation) < fabs(item.Deviation); }
	bool operator>(CCalculationViewItem item) { 
		return fabs(Deviation) > fabs(item.Deviation); }
};

class CCalculationListView : public CMFCListCtrl
{
	DECLARE_DYNAMIC(CCalculationListView)

public:
	std::vector<CCalculationViewItem>* m_items;

public:
	CCalculationListView();
	virtual ~CCalculationListView();

protected:
	DECLARE_MESSAGE_MAP()	

public:
	afx_msg void OnNMClick(NMHDR *pNMHDR, LRESULT *pResult);

protected:
	virtual void PreSubclassWindow();
public:
	void SetDataSource(std::vector<CCalculationViewItem>* resistors);
	void Update();

	afx_msg void OnLvnDeleteallitems(NMHDR *pNMHDR, LRESULT *pResult);
};


