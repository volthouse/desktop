// CalculationCtrlPage.cpp : implementation file
//

#include "stdafx.h"
#include "LoadCalc.h"
#include "CalculationCtrlPage.h"

// CCalculationCtrlPage dialog

IMPLEMENT_DYNAMIC(CCalculationCtrlPage, CPropertyPage)

CCalculationCtrlPage::CCalculationCtrlPage()
	: CPropertyPage(CCalculationCtrlPage::IDD)
{

}

CCalculationCtrlPage::~CCalculationCtrlPage()
{
}

void CCalculationCtrlPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_CALCULATIONPAGE_RESULT_LISTVIEW, m_resultListView);
	DDX_Control(pDX, IDC_CALCULATIONPAGE_LOADVALUE_EDIT, m_searchValueEdit);
	DDX_Control(pDX, IDC_CALCULATIONPAGE_VIVALUE_EDIT, m_viValueEdit);
	DDX_Control(pDX, IDC_CALCULATIONPAGE_PVALUES_EDIT, m_powerValuesEdit);
	DDX_Control(pDX, IDC_CALCULATIONPAGE_VOLTAGE_RADIO, m_voltageRadio);
	DDX_Control(pDX, IDC_CALCULATIONPAGE_CURRENT_RADIO, m_currentRadio);
	DDX_Control(pDX, IDC_CALCULATIONPAGE_MODE_COMBO, m_circuitryCombo);
}


BEGIN_MESSAGE_MAP(CCalculationCtrlPage, CPropertyPage)
	ON_BN_CLICKED(IDC_CALCULATIONPAGE_CALC_BTN, &CCalculationCtrlPage::OnBnClickedCalculationPageCalcBtn)
	ON_MESSAGE(WM_ONITEMCLICKEDMESSAGE, OnItemClicked)
	ON_MESSAGE(WM_ONCALCULATIONENDED, OnCalculationEnded)
	ON_BN_CLICKED(IDC_CALCULATIONPAGE_VOLTAGE_RADIO, &CCalculationCtrlPage::OnBnClickedCalculationPageVoltageRadio)
	ON_BN_CLICKED(IDC_CALCULATIONPAGE_CURRENT_RADIO, &CCalculationCtrlPage::OnBnClickedCalculationPageCurrentRadio)
	ON_EN_CHANGE(IDC_CALCULATIONPAGE_VIVALUE_EDIT, &CCalculationCtrlPage::OnEnChangeCalculationPageVivalueEdit)
	ON_CBN_SELCHANGE(IDC_CALCULATIONPAGE_MODE_COMBO, &CCalculationCtrlPage::OnCbnSelchangeCalculationpageModeCombo)
END_MESSAGE_MAP()


// CCalculationCtrlPage message handlers

BOOL CCalculationCtrlPage::OnInitDialog()
{
	CPropertyPage::OnInitDialog();
	
	m_currentItemIdx = -1;
	m_voltageRadio.SetCheck(TRUE);
	m_resultListView.SetDataSource(&m_viewItems);

	m_circuitryCombo.AddString(_T("Parallel"));
	m_circuitryCombo.AddString(_T("Series"));
	m_circuitryCombo.SetCurSel(0);

	m_bar.Create(this); //Status bar
	UINT BASED_CODE indicators[] = { ID_STATUS};
	m_bar.SetIndicators(indicators,1); //Anzahl panes 
	CRect rect;
	GetClientRect(&rect);
	m_bar.SetPaneInfo(0,ID_STATUS, SBPS_NORMAL,rect.Width());      	
	RepositionBars(AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, ID_STATUS);

	//m_searchValueEdit.SubclassDlgItem(IDC_CALCULATIONPAGE_LOADVALUE_EDIT, this);
	m_searchValueEdit.DragAcceptFiles(TRUE);

	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}

void CCalculationCtrlPage::OnBnClickedCalculationPageCalcBtn()
{
	if(m_calcThread != NULL) {
		m_calcThread->Cancel();	
	}

	m_viewItems.clear();
	m_bar.SetPaneText(0, _T("Computing..."));

	float searchValue = m_searchValueEdit.GetFloatValue();

	m_calcThread = 
		(CCalculationThread*)AfxBeginThread(RUNTIME_CLASS(CCalculationThread));
	m_calcThread->m_bAutoDelete = false;
	m_calcThread->m_parent = this;
	m_calcThread->SetData(searchValue, theApp.m_circuitry, theApp.GetCurrentSet());
	m_calcThread->ResumeThread();
}

LRESULT CCalculationCtrlPage::OnItemClicked(WPARAM wParam, LPARAM lParam)
{
	m_currentItemIdx = (int)wParam;
	CalcPower(m_currentItemIdx);
	return 0;
}

void CCalculationCtrlPage::OnBnClickedCalculationPageVoltageRadio()
{
	CalcPower(m_currentItemIdx);
}

void CCalculationCtrlPage::OnBnClickedCalculationPageCurrentRadio()
{
	CalcPower(m_currentItemIdx);
}

void CCalculationCtrlPage::OnEnChangeCalculationPageVivalueEdit()
{
	CalcPower(m_currentItemIdx);
}

void CCalculationCtrlPage::OnCbnSelchangeCalculationpageModeCombo()
{
	if(m_circuitryCombo.GetCurSel()==0)
		theApp.m_circuitry = Parallel;
	else
		theApp.m_circuitry = Series;
}

void CCalculationCtrlPage::CalculateParallelPower(int itemIdx) 
{
	if((itemIdx < 0) || (itemIdx > (int)m_viewItems.size())) return;

	float viValue = m_viValueEdit.GetFloatValue();

	CResistor resistor = m_viewItems[itemIdx].Resistor;
	CString s;
	float totalPower = 0;

	for(int i=0; i < (int)resistor.Resistors.size(); ++i) {
		float power = 0;
		if(m_voltageRadio.GetState()) {
			power = (viValue * viValue) / resistor.Resistors[i];
		} else {
			power = (viValue * viValue) * resistor.Resistors[i];
		}
		totalPower += power;
		if(power != 0) {
			s.AppendFormat(
				_T("%.0f Ohm - %.3f W"), resistor.Resistors[i], power
			);
		} else {
			s.AppendFormat(_T("%.0f Ohm"), resistor.Resistors[i]);
		}
		s.Append(L"\r\n");
	}

	if(totalPower != 0) {
		s.AppendFormat(_T("Total: %.3f W"), totalPower);
	}
	m_powerValuesEdit.SetWindowTextW(s);
}

void CCalculationCtrlPage::CalculateSeriesPower(int itemIdx)
{
	if((itemIdx < 0) || (itemIdx > (int)m_viewItems.size())) return;

	float viValue = m_viValueEdit.GetFloatValue();

	CResistor resistor = m_viewItems[itemIdx].Resistor;
	CString s;
	float totalPower = 0;
	float current = 0;
	
	if(m_voltageRadio.GetState()) {
		current = viValue / resistor.Value;
	} else {
		current = viValue;
	}

	for(int i=0; i < (int)resistor.Resistors.size(); ++i) {		
		float power = (current * current) * resistor.Resistors[i];		
		totalPower += power;
		if(power != 0) {
			s.AppendFormat(
				_T("%.0f Ohm - %.3f W"), resistor.Resistors[i], power
			);
		} else {
			s.AppendFormat(_T("%.0f Ohm"), resistor.Resistors[i]);
		}
		s.Append(L"\r\n");
	}

	if(totalPower != 0) {
		s.AppendFormat(_T("Total: %.3f W"), totalPower);
	}
	m_powerValuesEdit.SetWindowTextW(s);
}

void CCalculationCtrlPage::CalcPower(int itemIdx)
{
	switch(theApp.m_circuitry) {
		case Parallel:
			CalculateParallelPower(itemIdx);
			break;
		case Series:
			CalculateSeriesPower(itemIdx);
			break;
	}
}

void CCalculationCtrlPage::OnAsyncCalculationEnded()
{
	CWnd::PostMessageW(WM_ONCALCULATIONENDED,0 ,0);
}

LRESULT CCalculationCtrlPage::OnCalculationEnded(WPARAM wParam, LPARAM lParam)
{
	std::vector<CResistor>::const_iterator it;

	m_viewItems.clear();
	if(m_calcThread->m_result.size() > 0) {
		for (it=m_calcThread->m_result.begin(); it!=m_calcThread->m_result.end(); ++it) {
			CResistor pr = (*it);
			CCalculationViewItem item(m_calcThread->m_resistance, pr);
			m_viewItems.push_back(item);
		}
		std::sort(m_viewItems.begin(), m_viewItems.end());
		m_bar.SetPaneText(0, _T(""));
	} else {
		m_bar.SetPaneText(0, _T("not available"));
	}
	m_resultListView.Update();

	return 0;
}