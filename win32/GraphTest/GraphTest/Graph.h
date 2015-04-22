#pragma once

#include "Signal.h"

// CGraph

class CSignalView {
public:
	float xMin;
	float xMax;
	float yMin;
	float yMax;

	float x1;
	float x2;
	float y1;
	float y2;

	void Reset(void) {
		x1 = xMin;
		x2 = xMax;
		y1 = yMin;
		y2 = yMax;
	}
};

class CGraph : public CStatic
{
	DECLARE_DYNAMIC(CGraph)

public:
	CGraph();
	virtual ~CGraph();

protected:
	DECLARE_MESSAGE_MAP()

	CSignal* m_pSignals[10];
	CSignalView m_SignalView[10];

	int		m_nSignalCount;
	int		m_RefSignalIdx;
	int		m_nSelectedCursor;
	bool	m_bShowDots;
	bool	m_bShowCursor;
	bool	m_bShowGrid;
	
	CPen	m_hPen;
	CPen*	m_hColorPens[4];
	CPen	m_hDevisionPen;
	CBrush	m_hBrush;
	CFont*	m_hFont;

	CRect	m_FocusRect;
	CRect	m_GraphRect;

	int		m_Cursors[2];

public:
	virtual void DrawItem(LPDRAWITEMSTRUCT /*lpDrawItemStruct*/);

	
	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
	afx_msg void OnLButtonUp(UINT nFlags, CPoint point);
	afx_msg void OnContextMenu(CWnd* /*pWnd*/, CPoint /*point*/);
	afx_msg void OnShowDots();
	afx_msg void OnGraphResetZoom();
	afx_msg void OnGraphShowCursor();
	afx_msg void OnGraphShowGrid();
	afx_msg void OnGraphCopyValuesToClipboard();

	virtual BOOL PreTranslateMessage(MSG* pMsg);

	void AddSignal(CSignal* pSignal);	
	void DrawGraph(CDC* pDc, CRect* pRect);
	void DrawCursor(CDC* pDc, CRect* pRect);
	void DrawFrame(CDC* pDc, CRect* pRect);
	void DrawGrid(CDC* pDc, CRect* pRect);
	void DrawZeroLine(CDC* pDc, CRect* pRect);
	void EnableDots(bool bEnable);
	void EnableGrid(bool bEnable);
	bool FindDivider(float min, float max, int* pSteps, float* pStepWidth);
	void ResetZoom(void);
	void SignalChanged(void);	
};
