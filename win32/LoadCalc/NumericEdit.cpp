#include "math.h"
#include "NumericEdit.h"

// CNumericEdit


IMPLEMENT_DYNAMIC(CNumericEdit, CEdit)

CNumericEdit::CNumericEdit()
{

}

CNumericEdit::~CNumericEdit()
{
}


BEGIN_MESSAGE_MAP(CNumericEdit, CEdit)
	ON_WM_KEYDOWN()
	ON_WM_CHAR()
	ON_WM_DROPFILES()
	ON_WM_CREATE()
END_MESSAGE_MAP()



// CNumericEdit message handlers

bool CNumericEdit::IsNumberLPC(LPCTSTR lpszText)
{
	/// From DLGFLOAT.CPP, function _AfxSimpleFloatParse
	ASSERT(lpszText != NULL);
	while (*lpszText == ' ' || *lpszText == '\t')
	  lpszText++;

	/*LPCTSTR signCheck = lpszText;

	while (*signCheck == '+' || *signCheck == '-')
	  signCheck++;

	if ((signCheck - lpszText) > 1)
		return false;

	if ((signCheck - lpszText) == 1)
		return true;*/
	
	TCHAR chFirst = lpszText[0];
	double d = _tcstod(lpszText, (LPTSTR*)&lpszText);
	if (d == 0.0 && chFirst != '0')
	  return false;   // could not convert
	/*while (*lpszText == ' ' || *lpszText == '\t')
	  lpszText++;*/

	if (*lpszText != '\0')
	  return false;   // not terminated properly

	return true;
}

void CNumericEdit::OnChar(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	TCHAR chChar = (TCHAR) nChar;
	if (_istprint(chChar) && !(::GetKeyState(VK_CONTROL)&0x80))
	{
		CString text;
		GetWindowText(text);
		text.AppendChar(chChar);	
		if(!IsNumberLPC(text)) return;
	}
	
	CEdit::OnChar(nChar, nRepCnt, nFlags);
}

FLOAT CNumericEdit::GetFloatValue() {
	CString text;
	GetWindowText(text);
	return (FLOAT)_wtof(text);	
}

void CNumericEdit::OnDropFiles(HDROP hDropInfo)
{
	// Get the number of pathnames that have been dropped
	WORD wNumFilesDropped = DragQueryFile(hDropInfo, -1, NULL, 0);

	CString firstFile="";

	if(wNumFilesDropped > 0) {
		// Get the number of bytes required by the file's full pathname
		WORD wPathnameSize = DragQueryFile(hDropInfo, 0, NULL, 0) + 1;
		
		firstFile = CString("", wPathnameSize);
		// Copy the pathname into the buffer
		DragQueryFile(hDropInfo, 0, firstFile.GetBuffer(), wPathnameSize);		
	}

	if(IsNumberLPC(firstFile)) {
		SetWindowText(firstFile);
	}

	// Free the memory block containing the dropped-file information
	DragFinish(hDropInfo);
}

int CNumericEdit::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	if (CEdit::OnCreate(lpCreateStruct) == -1)
		return -1;
	
	DragAcceptFiles(TRUE);
	
	return 0;
}
