#include "averagecalculator.h"
#include "plot.h"
#include "data.h"
#include <QtGlobal>

AverageCalculator::AverageCalculator(QObject *parent) : Calculator(parent)
{   
}

void AverageCalculator::setTracker(CurveTracker* t1, CurveTracker* t2)
{
    this->t1 = t1;
    this->t2 = t2;

    //connect(t1, SIGNAL(moved(QPointF)), this, SLOT(calc(QPointF)));
    //connect(t2, SIGNAL(moved(QPointF)), this, SLOT(calc(QPointF)));
}

double AverageCalculator::result()
{
    return 0;
}

double AverageCalculator::calc(Data* data, int x1, int x2)
{
    double sum = 0.0;
    int n = 0;

    for(int i = x1; i < x2; i++) {
        QPointF p = data->sample(i);
        sum += p.y();
        n++;
    }

    qInfo() << "Test" << "x1:" << x1 << ", x2" << x2 << ", sum:" << sum << ", n:" << n << "avg:" << (sum / n);

    if(n > 0 || n < 0) {
        return sum / n;
    }

    return 0;
}

