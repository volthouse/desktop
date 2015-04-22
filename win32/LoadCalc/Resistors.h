#pragma once

#include "afx.h"
#include "Math.h"
#include <vector>
#include <algorithm>

enum ResistorCircuitry {
	None,
	Series,
	Parallel
};

class CResistor {

public:
	int ID;
	float Value;
	float Conductance;
	ResistorCircuitry Circuitry;
	std::vector<float> Resistors;
	
	CResistor() : Value(0), Conductance(0), Circuitry(None)  { }
	CResistor(int id, float value):ID(id), Value(value), Conductance(1/value) {}
	CResistor(ResistorCircuitry circuitry, std::vector<float>* resistors):
			Value(0), Conductance(0), Circuitry(circuitry) {	
		if(Circuitry == Parallel) {
			for(int i = 0; i < (int)resistors->size(); ++i) {
				float r = resistors->at(i);
				Resistors.push_back(r);
				Conductance += 1 / r;
			}
			Value = 1 / Conductance;
		} else if(Circuitry == Series) {
			for(int i = 0; i < (int)resistors->size(); ++i) {
				float r = resistors->at(i);
				Resistors.push_back(r);
				Value += r;
			}
		}
	}	
	bool operator<(CResistor r) { return Value < r.Value; }
	bool operator>(CResistor r) { return Value > r.Value; }
	bool operator==(CResistor resistor) { return Value == resistor.Value; }
	bool operator==(float r) { return Value == r; }
};

//class CResistor : public CResistor {
//public:		
//	std::vector<float> Resistors;
//
//	CResistor() { }	
//	CResistor(std::vector<float>* resistors) {		
//		for(int i = 0; i < (int)resistors->size(); ++i) {
//			float r = resistors->at(i);
//			Resistors.push_back(r);
//			Conductance += 1 / r;
//		}
//		Value = 1 / Conductance;
//	}	
//	
//	bool operator==(CResistor resistor) { 
//		return Value == resistor.Value; }
//	bool operator==(float r) { 
//		return Value == r; }
//};

class CResistorSet {
public:
	CString Name;
	std::vector<CResistor> m_resistors;
};

class CResistorImport {
public:
	void Import(LPCTSTR path, std::vector<CResistor>* resistors) {
		CFile f;
		CFileException e;
		if(f.Open(path, CFile::modeRead, &e)) {
			UINT nBytes = (UINT)f.GetLength();
			if(nBytes > 0) {															
				LPBYTE pData = new BYTE[nBytes + sizeof(WCHAR)];
				ZeroMemory(pData, nBytes + sizeof(WCHAR));
				f.Read(pData, nBytes+1);
				CString s(pData);
				ParseString(s, resistors);
			}
		}
	}

	void ParseString(CString s, std::vector<CResistor>* resistors) {
		int n = 0;
		CString st;

		st = s.Tokenize(_T(";"), n);
		while(!st.IsEmpty()) {
			resistors->push_back(CResistor(resistors->size(), (float)_wtof(st)));
			st = s.Tokenize(_T(";"), n);		
		}
		std::sort(resistors->begin(), resistors->end());
	}
};

class CCircuitCalculator {
	
public:	

	void SelectResistors(DWORD n, std::vector<CResistor> *resistors, std::vector<float>* selectedResistors)
	{
		selectedResistors->clear();
		for(int i=0; i < (int)resistors->size(); i++) {
			if(1 << i & n) {
				selectedResistors->push_back(resistors->at(i).Value);					
			}
		}
	}

	float CalcParallel(DWORD n, std::vector<CResistor>* resistors)
	{
		float conductance = 0;
		for(int i=0; i < (int)resistors->size(); i++) {
			if(1 << i & n) {
				conductance += 1 / resistors->at(i).Value;
			}
		}
		return (1.0f / conductance);
	}

	float CalcSeries(DWORD n, std::vector<CResistor>* resistors)
	{
		float resistance = 0;
		for(int i=0; i < (int)resistors->size(); i++) {
			if(1 << i & n) {
				resistance += resistors->at(i).Value;
			}
		}
		return resistance;
	}

	float Calc(ResistorCircuitry circuitry, DWORD n, std::vector<CResistor>* resistors) {
		switch(circuitry) {
			case Parallel: return CalcParallel(n, resistors);
			case Series:   return CalcSeries(n, resistors);
		}
		return 0;
	}

	bool FindLoad(float resistor, ResistorCircuitry circuitry, std::vector<CResistor> *resistors, 
		std::vector<CResistor> *result)
	{		
		std::vector<CResistor>::const_iterator it;
		DWORD nResistors = (DWORD)(1 << resistors->size());											

		for(float tolerance = 0.01f; tolerance < 1.0f; tolerance += 0.01f) {		
			float rmin = resistor * (1 - tolerance);
			float rmax = resistor * (1 + tolerance);
			result->clear();
			for(DWORD n = 1; n < nResistors; n++) { // n = n'te Kombination
				float r = Calc(circuitry, n, resistors);				
				if(r > rmin && r < rmax) {					
					it = std::find(result->begin(), result->end(), r);
					if(it == result->end()) {
						std::vector<float> selected;
						SelectResistors(n, resistors, &selected);
						CResistor resistor(circuitry, &selected);
						result->push_back(resistor);
					}
				}
			}
			if(result->size() > 0) break;
		}

		return result->size() > 0;
	}
};