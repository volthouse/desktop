// ResistorsCtrlPage.cpp : implementation file
//

#include "stdafx.h"
#include "LoadCalc.h"
#include "ResistorsCtrlPage.h"


// CResistorsCtrlPage dialog

IMPLEMENT_DYNAMIC(CResistorsCtrlPage, CPropertyPage)

CResistorsCtrlPage::CResistorsCtrlPage()
	: CPropertyPage(CResistorsCtrlPage::IDD)
{

}

CResistorsCtrlPage::~CResistorsCtrlPage()
{
}

void CResistorsCtrlPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_RESISTORSPAGE_LISTVIEW, m_listView);
	DDX_Control(pDX, IDC_RESISTORSPAGE_RESISTORS_COMBO1, m_setCombo);
}


BEGIN_MESSAGE_MAP(CResistorsCtrlPage, CPropertyPage)
	ON_CBN_SELCHANGE(IDC_RESISTORSPAGE_RESISTORS_COMBO1, &CResistorsCtrlPage::OnCbnSelchangeResistorspageResistorsCombo1)
END_MESSAGE_MAP()


// CResistorsCtrlPage message handlers

BOOL CResistorsCtrlPage::OnInitDialog()
{
	CPropertyPage::OnInitDialog();	
	
	InitializeResistorSetCombo();
	SetResistorSet(0);

	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}

void CResistorsCtrlPage::OnCbnSelchangeResistorspageResistorsCombo1()
{
	SetResistorSet(m_setCombo.GetCurSel());	
}

void CResistorsCtrlPage::SetResistorSet(int id)
{
	theApp.m_currentSet =id;

	m_listView.SetDataSource(theApp.GetCurrentSet());
	m_listView.Update();
}

void CResistorsCtrlPage::InitializeResistorSetCombo(void)
{
	for(int i = 0; i < (int)theApp.m_resistorSets.size(); ++i) {
		m_setCombo.AddString(theApp.m_resistorSets[i].Name);
	}
	m_setCombo.SetCurSel(0);
}
