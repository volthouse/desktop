// CalculationListView.cpp : implementation file
//

#include "stdafx.h"
#include "LoadCalc.h"
#include "CalculationListView.h"


// CCalculationListView

IMPLEMENT_DYNAMIC(CCalculationListView, CMFCListCtrl)

CCalculationListView::CCalculationListView()
{	
}

CCalculationListView::~CCalculationListView()
{
}


BEGIN_MESSAGE_MAP(CCalculationListView, CMFCListCtrl)
	ON_NOTIFY_REFLECT(NM_CLICK, &CCalculationListView::OnNMClick)
	ON_NOTIFY_REFLECT(LVN_DELETEALLITEMS, &CCalculationListView::OnLvnDeleteallitems)
END_MESSAGE_MAP()


// CCalculationListView message handlers

void CCalculationListView::OnNMClick(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMITEMACTIVATE pNMItemActivate = reinterpret_cast<LPNMITEMACTIVATE>(pNMHDR);
	
	GetParent()->SendMessage(
		WM_ONITEMCLICKEDMESSAGE, (WPARAM)pNMItemActivate->iItem, 0
	);
	*pResult = 0;
}

void CCalculationListView::PreSubclassWindow()
{
	LVCOLUMN col;
	col.mask = LVCF_FMT | LVCF_TEXT | LVCF_WIDTH ;
	col.fmt = LVCFMT_LEFT;
	col.cx = 70;
	col.pszText = L"Resistance";
	InsertColumn(0, &col);

	col.mask = LVCF_FMT | LVCF_TEXT | LVCF_WIDTH ;
	col.fmt = LVCFMT_LEFT;
	col.cx = 70;
	col.pszText = L"Delta %";
	InsertColumn(1, &col);

	col.mask = LVCF_FMT | LVCF_TEXT | LVCF_WIDTH ;
	col.fmt = LVCFMT_LEFT;
	col.cx = 378;
	col.pszText = L"Resistors";
	InsertColumn(2, &col);	

	SetExtendedStyle(
		LVS_EX_FULLROWSELECT |  LVS_EX_DOUBLEBUFFER |
		LVS_EX_GRIDLINES | LVS_EX_HEADERDRAGDROP);
	
	EnableGroupView(FALSE);

	CMFCListCtrl::PreSubclassWindow();
}

void CCalculationListView::SetDataSource(std::vector<CCalculationViewItem>* items)
{
	m_items = items;
}

void CCalculationListView::Update() 
{
	std::vector<CCalculationViewItem>::const_iterator it;
	CString s;		

	DeleteAllItems();

	for (it=m_items->begin(); it!=m_items->end(); ++it) {
		CString s;		
		LVITEM item;
		
		item.mask = LVIF_TEXT ;
		item.iItem = GetItemCount() + 1;
		item.iSubItem = 0;		
		item.pszText = L"";

		int nItem = InsertItem(&item);

		s.Format(_T("%.3f"), (*it).Resistor.Value);
		SetItemText(nItem, 0, s);
		s.Format(_T("%1.3f"), (*it).Deviation * 100);
		SetItemText(nItem, 1, s);
		s.Empty();
		for(int i=0; i < (int)(*it).Resistor.Resistors.size(); i++) {
			s.AppendFormat(_T("%.0f, "), (*it).Resistor.Resistors[i]);
		}
		s.Delete(s.GetLength() - 2, 2);
		SetItemText(nItem, 2, s);
	}	
}

void CCalculationListView::OnLvnDeleteallitems(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);
	*pResult = 0;
}
