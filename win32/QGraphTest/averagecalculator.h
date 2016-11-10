#ifndef AVERAGECALCULATOR_H
#define AVERAGECALCULATOR_H

#include <QObject>
#include "curvetracker.h"
#include "calculator.h"
#include "data.h"

class AverageCalculator : public Calculator
{
    Q_OBJECT
public:
    explicit AverageCalculator(QObject *parent = 0);

    void setTracker(CurveTracker* t1, CurveTracker* t2);

    virtual double result();

signals:

public slots:
    double calc(Data* data, int x1, int x2);

private:
    CurveTracker* t1;
    CurveTracker* t2;
};

#endif // AVERAGECALCULATOR_H
