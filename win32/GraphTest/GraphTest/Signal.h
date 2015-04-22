#pragma once

class CSignal
{
protected:
	float m_Wave[3*360];
public:
	CSignal(void);
	~CSignal(void);

	virtual float GetYMin(int fromIdx = 0, int toIdx = 0);
	virtual float GetYMax(int fromIdx = 0, int toIdx = 0);

	virtual float GetXMin(void);
	virtual float GetXMax(void);	
	virtual float GetYAt(int pos);
	virtual int GetCount(void);	
};

class CCosine : public CSignal 
{

public:
	CCosine(void);
	~CCosine(void);

	virtual int GetCount(void);
	virtual float GetXMax(void);
};
