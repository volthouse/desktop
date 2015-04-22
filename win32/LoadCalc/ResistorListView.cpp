// ResistorListView.cpp : implementation file
//

#include "stdafx.h"
#include "LoadCalc.h"
#include "ResistorListView.h"


// CResistorListView

IMPLEMENT_DYNAMIC(CResistorListView, CMFCListCtrl)

CResistorListView::CResistorListView()
{
}

CResistorListView::~CResistorListView()
{
}


BEGIN_MESSAGE_MAP(CResistorListView, CMFCListCtrl)
END_MESSAGE_MAP()


// CResistorListView message handlers

void CResistorListView::PreSubclassWindow()
{
	LVCOLUMN col;
	col.mask = LVCF_FMT | LVCF_TEXT | LVCF_WIDTH;
	col.fmt = LVCFMT_LEFT;
	col.cx = 50;
	col.pszText = L"Index";
	InsertColumn(0, &col);

	col.mask = LVCF_FMT | LVCF_TEXT | LVCF_WIDTH;
	col.fmt = LVCFMT_LEFT;
	col.cx = 140;
	col.pszText = L"Resistance";
	InsertColumn(1, &col);

	SetExtendedStyle(
		LVS_EX_FULLROWSELECT |  LVS_EX_DOUBLEBUFFER |
		LVS_EX_GRIDLINES | LVS_EX_HEADERDRAGDROP);

	EnableGroupView(FALSE);

	CMFCListCtrl::PreSubclassWindow();
}

void CResistorListView::SetDataSource(std::vector<CResistor>* resistors)
{
	m_resistors = resistors;
}

void CResistorListView::Update()
{
	std::vector<CResistor>::const_iterator it;
	CString s;

	DeleteAllItems();

	for (it=m_resistors->begin(); it!=m_resistors->end(); ++it) {
		CString s;
		LVITEM item;
		int count = GetItemCount() + 1;

		item.mask = LVIF_TEXT;
		item.iItem = count;
		item.iSubItem = 0;
		item.pszText = L"";

		int nItem = InsertItem(&item);

		s.Format(_T("%0.d"), count);
		SetItemText(nItem, 0, s);
		s.Format(_T("%.3f"), (*it).Value);
		SetItemText(nItem, 1, s);
	}
}

