#pragma once

#include "Resistors.h"
#include <vector>


// CResistorListView

class CResistorListView : public CMFCListCtrl
{
	DECLARE_DYNAMIC(CResistorListView)

public:
	std::vector<CResistor>* m_resistors;

public:
	CResistorListView();
	virtual ~CResistorListView();

protected:
	DECLARE_MESSAGE_MAP()	

protected:
	virtual void PreSubclassWindow();
public:
	void SetDataSource(std::vector<CResistor>* resistors);
	void Update();

};





