#include "StdAfx.h"
#include "Signal.h"
#include <math.h>
#include <cfloat>

#define M_PI       3.14159265358979323846f
#define NPERIODS   1

CSignal::CSignal(void)
{
	int n = 0;
	for(int i = -9; i < 10; i++) {
		m_Wave[n++] = i;		
	}
}

CSignal::~CSignal(void)
{
}

float CSignal::GetYMin(int fromIdx, int toIdx)
{
	if(toIdx == 0 && toIdx == 0) {
		fromIdx = 0; 
		toIdx = GetCount();
	}

	float r = FLT_MAX;

	for(int i = fromIdx; i < toIdx; i++)
	{
		if(m_Wave[i] < r) {
			r = m_Wave[i];
		}
	}

	return r;
}

float CSignal::GetYMax(int fromIdx, int toIdx)
{
	if(toIdx == 0 && toIdx == 0) {
		fromIdx = 0; 
		toIdx = GetCount();
	}

	float r = FLT_MIN;

	for(int i = fromIdx; i < toIdx; i++)
	{
		if(m_Wave[i] > r) {
			r = m_Wave[i];
		}
	}

	return r;
}

float CSignal::GetXMin(void)
{
	return 0.0f;
}

float CSignal::GetXMax(void)
{
	return (GetCount() -1) * 0.005f;//1.0f;
}

float CSignal::GetYAt(int pos)
{
	if(pos >= 0 && pos < GetCount()) {
		return m_Wave[pos];
	}

	return 0.0f;
}

int CSignal::GetCount(void)
{
	return 19;
}



// Cos ///////////////////////////////////////////

float CCosine::GetXMax(void)
{
	return 1.0f;
}

int CCosine::GetCount(void)
{
	return NPERIODS*360;
}

CCosine::CCosine(void)
{	
	for(int i = 0; i < NPERIODS*360; i++) {
		m_Wave[i] = cos(i * 2.0f * M_PI / 360.0f);		
	}
}

CCosine::~CCosine(void)
{
}


//#include "StdAfx.h"
//#include "Signal.h"
//#include <math.h>
//#include <cfloat>
//
//#define M_PI       3.14159265358979323846f
//#define NPERIODS   2
//
//CSignal::CSignal(void)
//{
//	for(int i = 0; i < NPERIODS*360; i++) {
//		m_Wave[i] = sin(i * 2.0f * M_PI / 360.0f);
//		/*m_Wave[i] += 0.5f * sin(3 * i * 2.0f * M_PI / 360.0f);
//		m_Wave[i] += 0.25f * sin(4 * i * 2.0f * M_PI / 360.0f);
//		m_Wave[i] += 0.125f * sin(5 * i * 2.0f * M_PI / 360.0f);
//		m_Wave[i] += 0.0625f * sin(6 * i * 2.0f * M_PI / 360.0f);*/
//	}
//}
//
//CSignal::~CSignal(void)
//{
//}
//
//float CSignal::GetYMin(int fromIdx, int toIdx)
//{
//	if(toIdx == 0 && toIdx == 0) {
//		fromIdx = 0; 
//		toIdx = GetCount();
//	}
//
//	float r = FLT_MAX;
//
//	for(int i = fromIdx; i < toIdx; i++)
//	{
//		if(m_Wave[i] < r) {
//			r = m_Wave[i];
//		}
//	}
//
//	return r;
//}
//
//float CSignal::GetYMax(int fromIdx, int toIdx)
//{
//	if(toIdx == 0 && toIdx == 0) {
//		fromIdx = 0; 
//		toIdx = GetCount();
//	}
//
//	float r = FLT_MIN;
//
//	for(int i = fromIdx; i < toIdx; i++)
//	{
//		if(m_Wave[i] > r) {
//			r = m_Wave[i];
//		}
//	}
//
//	return r;
//}
//
//float CSignal::GetXMin(void)
//{
//	return 0.0f;
//}
//
//float CSignal::GetXMax(void)
//{
//	return 1.0f;
//}
//
//float CSignal::GetYAt(int pos)
//{
//	if(pos >= 0 && pos < GetCount()) {
//		return m_Wave[pos];
//	}
//
//	return 0.0f;
//}
//
//int CSignal::GetCount(void)
//{
//	return NPERIODS*360;
//}
//
//
//
//// Cos ///////////////////////////////////////////
//
//float CCosine::GetXMax(void)
//{
//	return 1.0f;
//}
//
//int CCosine::GetCount(void)
//{
//	return NPERIODS*360;
//}
//
//CCosine::CCosine(void)
//{	
//	for(int i = 0; i < NPERIODS*360; i++) {
//		m_Wave[i] = cos(i * 2.0f * M_PI / 360.0f);		
//	}
//}
//
//CCosine::~CCosine(void)
//{
//}
