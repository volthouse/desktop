#ifndef DATAVIEW_H
#define DATAVIEW_H

#include <QWidget>
#include <QDoubleSpinBox>
#include <QTableWidget>
#include <QList>

#include "data.h"
#include "calculator.h"

class DataView : public QWidget
{
    Q_OBJECT

private:
    QList<QWidget*> resultWidgets;
    QList<Calculator*> calculators;
    QTableWidget* tableWidget;


    Data* data;
    int rangeFromIndex;
    int rangeToIndex;

public:
    explicit DataView(QWidget *parent = 0);

    void attachData(Data* data);
    void addCalculator(Calculator* calculator);
    void setRange(int fromIndex, int toIndex);

signals:

public slots:


};

#endif // DATAVIEW_H
