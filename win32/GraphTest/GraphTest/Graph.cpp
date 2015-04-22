// Graph.cpp : implementation file
//

#include "stdafx.h"
#include "Graph.h"
#include "math.h"
#include "MemDC.h"
#include "GraphTest.h"

static int map_fi(float x, float in_min, float in_max, int out_min, int out_max, bool clamp = false)
{
  int y = static_cast<int>((x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min);
  if(clamp) {
	  if(y > out_max)
		  y = out_max;
	  else if(y < out_min)
		  y = out_min;
  }
  return y;
}

static float map_if(int x, int in_min, int in_max, float out_min, float out_max)
{
  return static_cast<float>((x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min);
}

static int map_i(int x, int in_min, int in_max, int out_min, int out_max)
{
  return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
}

static float map_f(float x, float in_min, float in_max, float out_min, float out_max)
{
  return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
}

// CGraph

IMPLEMENT_DYNAMIC(CGraph, CStatic)

CGraph::CGraph() : m_nSignalCount(0), m_FocusRect(0, 0, 0, 0), m_RefSignalIdx(0), m_bShowGrid(false),
	m_bShowDots(false), m_bShowCursor(false), m_nSelectedCursor(0)
{
	m_hFont = new CFont();
	m_hFont->CreateFontW(-9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, NULL);
	m_hPen.CreatePen(PS_SOLID, 3, RGB(128,0,0));			//RED 
	m_hDevisionPen.CreatePen(PS_DOT,1,RGB(96,96,96));
	m_hColorPens[0] = new CPen;
	m_hColorPens[0]->CreatePen(PS_SOLID, 1, RGB(255,255,0));	//Yellow
	m_hColorPens[1] = new CPen;
	m_hColorPens[1]->CreatePen(PS_SOLID, 1, RGB(0,255,0));		//Green
	m_hColorPens[2] = new CPen;
	m_hColorPens[2]->CreatePen(PS_SOLID, 1, RGB(255,0,255));	//Magenty
	m_hColorPens[3] = new CPen;
	m_hColorPens[3]->CreatePen(PS_SOLID, 1, RGB(148,148,148));	//Gray
	m_hBrush.CreateSolidBrush(RGB(0,0,0));
}

CGraph::~CGraph()
{
}


BEGIN_MESSAGE_MAP(CGraph, CStatic)
	ON_WM_LBUTTONDOWN()
	ON_WM_MOUSEMOVE()
	ON_WM_LBUTTONUP()
	ON_WM_CONTEXTMENU()
	ON_COMMAND(ID_GRAPH_SHOWDOTS, &CGraph::OnShowDots)
	ON_COMMAND(ID_GRAPH_RESETZOOM, &CGraph::OnGraphResetZoom)
	ON_COMMAND(ID_GRAPH_SHOWCURSOR, &CGraph::OnGraphShowCursor)
	ON_COMMAND(ID_GRAPH_SHOWGRID, &CGraph::OnGraphShowGrid)
	ON_COMMAND(ID_GRAPH_COPYVALUESTOCLIPBOARD, &CGraph::OnGraphCopyValuesToClipboard)
END_MESSAGE_MAP()



// CGraph message handlers



void CGraph::DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct)
{
	CRect rect;
	
	GetClientRect(&rect);
	m_GraphRect.SetRect(10, 10, rect.right - 10, rect.bottom - 10);
	
	CMemDCEx dc(GetDC(), &rect);
	dc.SetBkMode(TRANSPARENT);	
	
	CBrush* hOldBrush = (CBrush*)dc.SelectObject(&m_hBrush);
	dc.FillRect(rect, &m_hBrush);
	
	DrawGraph(&dc, &m_GraphRect);
	if(m_bShowGrid) {
		DrawGrid(&dc, &m_GraphRect);
	}
	DrawZeroLine(&dc, &m_GraphRect);
	DrawFrame(&dc, &rect);

	if(m_bShowCursor) {
		DrawCursor(&dc, &m_GraphRect);
	}

	dc.DrawFocusRect(&m_FocusRect);
	dc.SelectObject(hOldBrush);
}

void CGraph::DrawGraph(CDC* pDc, CRect* pRect)
{
	// Referenz Bereich
	float xMin = m_SignalView[m_RefSignalIdx].x1;
	float xMax = m_SignalView[m_RefSignalIdx].x2;	
	
	CBrush dotBrush(RGB(255,255,0));

	// Signal zeichnen
	for(int s = 0; s < m_nSignalCount; s++)
	{
		pDc->SelectObject(m_hColorPens[s]);		

		CSignal* pSignal = m_pSignals[s];
		CSignalView* pSignalView = &m_SignalView[s];

		//float xInterval = (pSignalView->xMax - pSignalView->xMin) / pSignal->GetCount();

		int idxMax = pSignal->GetCount() - 1;
		int idx1 = map_fi(xMin, pSignalView->xMin, pSignalView->xMax, 0, idxMax, true);
		int idx2 = map_fi(xMax, pSignalView->xMin, pSignalView->xMax, 0, idxMax, true);
				
		int i = idx1;
		
		float xPhysical = map_if(i, 0, idxMax, pSignalView->xMin, pSignalView->xMax);
		float yPhysical = pSignal->GetYAt(i);

		int x = map_fi(xPhysical, xMin, xMax, pRect->left, pRect->right);
		int y = map_fi(yPhysical, pSignalView->y1, pSignalView->y2, pRect->bottom, pRect->top);
		pDc->MoveTo(x, y);
		if(m_bShowDots) {
			CRect d(x-2, y-2, x+2, y+2);
			pDc->FillRect(&d, &dotBrush);
		}

		for(i = idx1 + 1; i <= idx2; i++)
		{
			xPhysical = map_if(i, 0, idxMax, pSignalView->xMin, pSignalView->xMax);
			yPhysical = pSignal->GetYAt(i);

			x = map_fi(xPhysical, xMin, xMax, pRect->left, pRect->right);
			y = map_fi(yPhysical, pSignalView->y1, pSignalView->y2, pRect->bottom, pRect->top);
			pDc->LineTo(x, y);

			if(m_bShowDots) {
				CRect d(x-2, y-2, x+2, y+2);
				pDc->FillRect(&d, &dotBrush);
			}
		}
	}
}

void CGraph::DrawGrid(CDC* pDc, CRect* pRect)
{	
	CPen pen(PS_DOT, 1, RGB(148, 148, 148));
	pDc->SelectObject(pen);	
	
	CSignalView* pSignalView = &m_SignalView[m_RefSignalIdx];	

	int nSteps = 0;
	float stepWidth = 0.0f;


	// x-Axis divider
	FindDivider(pSignalView->xMin, pSignalView->xMax, &nSteps, &stepWidth);	
	for(int i = 0; i < nSteps; i++) {
		float xPhysical = i * stepWidth;
		if(xPhysical > pSignalView->x1 && xPhysical < pSignalView->x2) {
			int x = map_fi(xPhysical, pSignalView->x1, pSignalView->x2, pRect->left, pRect->right);
			pDc->MoveTo(x, pRect->top);
			pDc->LineTo(x, pRect->bottom);
		}
	}

	// y-Axis divider
	FindDivider(pSignalView->yMin, pSignalView->yMax, &nSteps, &stepWidth);		
	for(int i = 0; i < nSteps; i++) {
		float yPhysical = i * stepWidth;
		if(yPhysical > pSignalView->y1 && yPhysical < pSignalView->y2) {
			int y = map_fi(yPhysical, pSignalView->y1, pSignalView->y2, pRect->bottom, pRect->top);
			pDc->MoveTo(pRect->left, y);
			pDc->LineTo(pRect->right, y);
		}
	}

	for(int i = 0; i < nSteps; i++) {
		float yPhysical = -i * stepWidth;
		if(yPhysical > pSignalView->y1 && yPhysical < pSignalView->y2) {
			int y = map_fi(yPhysical, pSignalView->y1, pSignalView->y2, pRect->bottom, pRect->top);
			pDc->MoveTo(pRect->left, y);
			pDc->LineTo(pRect->right, y);
		}
	}
}

void CGraph::DrawZeroLine(CDC* pDc, CRect* pRect)
{	
	CSignalView* pSignalView = &m_SignalView[m_RefSignalIdx];

	// Zero
	CPen pen1(PS_SOLID, 1, RGB(148, 148, 148));
	pDc->SelectObject(pen1);

	int y = map_fi(0.0f, pSignalView->y1, pSignalView->y2, pRect->bottom, pRect->top);
	pDc->MoveTo(pRect->left, y);
	pDc->LineTo(pRect->right, y);
}

void CGraph::DrawCursor(CDC* pDc, CRect* pRect)
{	
	CSignalView* pSignalView = &m_SignalView[m_RefSignalIdx];

	const int xPos[3] = { 10, 150, 290 };
	float xPhysical[2] = { 0.0f };
	float yPhysical[2] = { 0.0f };
	CString s;

	pDc->SetTextColor(RGB(255,255,255));

	CRect rect;
	GetClientRect(&rect);
	
	for(int i = 0; i < 2; i++) 
	{
		CSignal* pSignal = m_pSignals[m_RefSignalIdx];
		int idxMax = pSignal->GetCount() - 1;
		xPhysical[i] = map_if(m_Cursors[i], pRect->left, pRect->right, pSignalView->x1, pSignalView->x2);
		int idx = map_fi(xPhysical[i], pSignalView->xMin, pSignalView->xMax, 0, idxMax) + 1;

		// y interpolieren
		float y1 = pSignal->GetYAt(idx - 1);
		float y2 = pSignal->GetYAt(idx);
		float x1 = map_if(idx - 1, 0, idxMax, pSignal->GetXMin(), pSignal->GetXMax());
		float x2 = map_if(idx, 0, idxMax, pSignal->GetXMin(), pSignal->GetXMax());

		yPhysical[i] = map_f(xPhysical[i], x1, x2, y1, y2);
				
		s.Format(L"X%d: %0.3e   Y%d: %0.3e", i + 1, xPhysical[i], i + 1, yPhysical[i]);		
		
		CPen pen(PS_SOLID, 1, RGB(128*i, 200, 0));
		pDc->SelectObject(pen);
		pDc->MoveTo(m_Cursors[i], pRect->top + 1);
		pDc->LineTo(m_Cursors[i], pRect->bottom - 1);

		CRect textRect(xPos[i], pRect->bottom, xPos[i] + 140, pRect->bottom + 10);
		pDc->SelectObject(m_hFont);
		pDc->DrawText(s, &textRect, 0);
	}

	s.Format(L"dX: %0.3e  dY: %0.3e", xPhysical[1]-xPhysical[0], yPhysical[1]-yPhysical[0]);

	CRect textRect(xPos[2], pRect->bottom, xPos[2] + 140, pRect->bottom + 10);
	pDc->SelectObject(m_hFont);
	pDc->DrawText(s, &textRect, 0);
}

void CGraph::DrawFrame(CDC* pDc, CRect* pRect)
{
	CRect b1(0, 0, pRect->right, 10);
	pDc->FillRect(&b1, &m_hBrush);
	CRect b2(0, pRect->bottom - 10, pRect->right, pRect->bottom);
	pDc->FillRect(&b2, &m_hBrush);
	CRect b3(0, 0, 10, pRect->bottom);
	pDc->FillRect(&b3, &m_hBrush);
	CRect b4(pRect->right - 10, 0, pRect->right, pRect->bottom);
	pDc->FillRect(&b4, &m_hBrush);

	CBrush b(RGB(255,255,255));	
	pDc->FrameRect(&m_GraphRect, &b);
}

void CGraph::AddSignal(CSignal* pSignal)
{
	m_pSignals[m_nSignalCount] = pSignal;	

	m_nSignalCount = (m_nSignalCount + 1) % 10;

	SignalChanged();
}

void CGraph::OnLButtonDown(UINT nFlags, CPoint point)
{
	SetFocus();
	if(nFlags & MK_SHIFT) {
		m_FocusRect.SetRect(point, point);	
	} else {
		int d1 = abs(m_Cursors[0] - point.x);
		int d2 = abs(m_Cursors[1] - point.x);
		if(d1 < d2) {
			m_nSelectedCursor = 0;
		} else {
			m_nSelectedCursor = 1;
		}
	}

	CStatic::OnLButtonDown(nFlags, point);
}

void CGraph::OnLButtonUp(UINT nFlags, CPoint point)
{
	if(m_FocusRect.left != m_FocusRect.right) {						
		for(int s = 0; s < m_nSignalCount; s++)
		{
			CSignalView* pSignalView = & m_SignalView[s];
			float x1 = map_if(m_FocusRect.left, m_GraphRect.left, m_GraphRect.right, pSignalView->x1, pSignalView->x2);
			float x2 = map_if(m_FocusRect.right, m_GraphRect.left, m_GraphRect.right, pSignalView->x1, pSignalView->x2);
			float y2 = map_if(m_FocusRect.top, m_GraphRect.bottom, m_GraphRect.top, pSignalView->y1, pSignalView->y2);
			float y1 = map_if(m_FocusRect.bottom, m_GraphRect.bottom, m_GraphRect.top, pSignalView->y1, pSignalView->y2);

			pSignalView->x1 = x1;
			pSignalView->x2 = x2;
			pSignalView->y2 = y2;
			pSignalView->y1 = y1;
		}
	
		m_FocusRect.SetRect(0, 0, 0, 0);
		Invalidate();
	}

	CStatic::OnLButtonUp(nFlags, point);
}

void CGraph::OnMouseMove(UINT nFlags, CPoint point)
{
	switch(nFlags) {
		case (MK_LBUTTON | MK_SHIFT):
			if(m_FocusRect.left != 0) {
				m_FocusRect.SetRect(m_FocusRect.TopLeft(), point);
			}
			Invalidate();
			break;
		case MK_LBUTTON:
			m_Cursors[m_nSelectedCursor] = point.x;
			Invalidate();
			break;
	}

	CStatic::OnMouseMove(nFlags, point);
}


BOOL CGraph::PreTranslateMessage(MSG* pMsg)
{
	switch(pMsg->message)
	{
		case WM_KEYDOWN: {
			switch(pMsg->wParam)
			{
				case VK_F5: {
					Invalidate();
					break;
				}
				case VK_ESCAPE: {
					ResetZoom();
					return true;
					break;
				}

				case VK_RIGHT: {
					m_Cursors[m_nSelectedCursor]++;
					Invalidate();
					return true;
					break;
				}

			   case VK_LEFT: {
					m_Cursors[m_nSelectedCursor]--;
					Invalidate();
					return true;
					break;
				}
			}
			break;
		}
		
	}
	
	return CStatic::PreTranslateMessage(pMsg);
}

bool CGraph::FindDivider(float min, float max, int* pSteps, float* pStepWidth)
{	
	float stepWidth = (max - min) / 10.0f;	
	float expof10 = floor(log10(stepWidth));
	stepWidth = pow(10.0f, expof10);	
	int n = static_cast<int>(ceil((max - min) / stepWidth));

	int d = abs(80 - n);
	int nSteps = 80;
	if(abs(75 - n) < d) {
		d = abs(75 - n);
		nSteps = 75;
	}
	if(abs(50 - n) < d) {
		d = abs(50 - n);
		nSteps = 50;
	}
	if(abs(40 - n) < d) {
		d = abs(40 - n);
		nSteps = 40;
	}
	if(abs(25 - n) < d) {
		d = abs(25 - n);
		nSteps = 25;
	}
	if(abs(20 - n) < d) {
		d = abs(20 - n);
		nSteps = 20;
	}
	if(abs(10 - n) < d) {
		d = abs(10 - n);
		nSteps = 10;		
	}
	
	stepWidth = nSteps * pow(10, expof10 - 1);

	*pSteps = 20;//nSteps;
	*pStepWidth = stepWidth;

	return nSteps > 0;
}

void CGraph::EnableDots(bool bEnable)
{
	m_bShowDots = bEnable;
	Invalidate();
}

void CGraph::EnableGrid(bool bEnable)
{
	m_bShowGrid = bEnable;
	Invalidate();
}

void CGraph::OnContextMenu(CWnd* pWnd, CPoint point)
{
	CMenu mnuPopupSubmit;
	mnuPopupSubmit.LoadMenu(IDR_GRAPH_CONTEXT_MENU);

	CMenu *mnuPopupMenu = mnuPopupSubmit.GetSubMenu(0);
		
	mnuPopupMenu->TrackPopupMenu(TPM_LEFTALIGN | TPM_RIGHTBUTTON, point.x, point.y, this);
}

void CGraph::OnShowDots()
{
	m_bShowDots = !m_bShowDots;
	Invalidate();
}

void CGraph::OnGraphResetZoom()
{
	ResetZoom();
}

void CGraph::OnGraphShowCursor()
{
	m_Cursors[0] = 20;
	m_Cursors[1] = 30;
	m_bShowCursor = !m_bShowCursor;
	Invalidate();
}

void CGraph::OnGraphShowGrid()
{
	m_bShowGrid = !m_bShowGrid;
	Invalidate();
}

void CGraph::ResetZoom(void)
{
	for(int s = 0; s < m_nSignalCount; s++)
	{
		m_SignalView[s].Reset();
	}
	Invalidate();
}

void CGraph::SignalChanged(void)
{
	for(int s = 0; s < m_nSignalCount; s++)
	{
		CSignal* pSignal = m_pSignals[s];
		CSignalView* pSignalView = &m_SignalView[s];

		pSignalView->xMin = pSignal->GetXMin();
		pSignalView->xMax = pSignal->GetXMax();

		pSignalView->yMin = pSignal->GetYMin();
		pSignalView->yMax = pSignal->GetYMax();

		float d = abs(pSignalView->yMax - pSignalView->yMin) * 0.05f;

		pSignalView->yMin -= d;
		pSignalView->yMax += d;

		pSignalView->x1 = pSignalView->xMin;
		pSignalView->x2 = pSignalView->xMax;

		pSignalView->y1 = pSignalView->yMin;
		pSignalView->y2 = pSignalView->yMax;
	}

	// Signal mit grössten X-Achsen Bereich suchen
	m_RefSignalIdx = 0;

	for(int s = 0; s < m_nSignalCount; s++)
	{	
		float range1 = m_SignalView[s].x2 - m_SignalView[s].x1;
		float range2 = m_SignalView[m_RefSignalIdx].x2 - m_SignalView[m_RefSignalIdx].x1;
		if(range1 > range2) {
			m_RefSignalIdx = s;
		}
	}
}

void CGraph::OnGraphCopyValuesToClipboard()
{
	if (!OpenClipboard() )	{
		AfxMessageBox( _T("Cannot open the Clipboard") );
		return;
	}
	// Remove the current Clipboard contents
	if( !EmptyClipboard() )	{
		AfxMessageBox( _T("Cannot empty the Clipboard") );
		return;
	}

	int count = 0;
	for(int s = 0; s < m_nSignalCount; s++)	{
		if(m_pSignals[s]->GetCount() > count) {
			count = m_pSignals[s]->GetCount();
		}
	}
		
	CString strClipboardData;
	for(int i = 0; i < count; i++) {
		for(int s = 0; s < m_nSignalCount; s++)	{
			CSignal* pSignal = m_pSignals[s];
			if(i < pSignal->GetCount()) {
				strClipboardData.AppendFormat(L"%f;", pSignal->GetYAt(i));			
			}
		}
		strClipboardData.Delete(strClipboardData.GetLength() - 1, 1);
		strClipboardData.Append(L"\r\n");
	}

	size_t iDataSize = sizeof(TCHAR)*(1 + strClipboardData.GetLength());

	HGLOBAL hDataPool = GlobalAlloc(GMEM_MOVEABLE, iDataSize);
	LPTSTR lptstrDataPoolCopy = (LPTSTR)GlobalLock(hDataPool);
	memcpy(lptstrDataPoolCopy, strClipboardData.GetBuffer(), iDataSize);
	GlobalUnlock(hDataPool);

#ifndef _UNICODE
    if ( NULL == ::SetClipboardData( CF_TEXT, hDataPool) )
#else
    if ( NULL == ::SetClipboardData( CF_UNICODETEXT, hDataPool) )
#endif
	{
		CString strMessage;
		strMessage.Format(_T("Unable to set Clipboard data, error: %d"), GetLastError());
		::AfxMessageBox( strMessage );
		::CloseClipboard();
		GlobalFree(hDataPool);
		return;
	}

    CloseClipboard();
}
