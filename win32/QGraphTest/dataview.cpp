#include <QLayout>
#include <QPushButton>

#include "dataview.h"
#include "averagecalculator.h"

DataView::DataView(QWidget *parent) : QWidget(parent)
{
    QVBoxLayout* vl = new QVBoxLayout(this);

    tableWidget = new QTableWidget();

    vl->addWidget(tableWidget);

    tableWidget->insertColumn(0);

    //QPushButton* calcButton = new QPushButton();
    //vl->addWidget(calcButton);

    //connect(calcButton, SIGNAL(clicked(bool)), this, SLOT(addCalcTest()));
}

void DataView::attachData(Data *data)
{
    this->data = data;

    QLocale l;

    for(int i = 0; i < (int)data->size(); i++) {
        tableWidget->insertRow(i);

        QString s = l.toString(data->sample(i).y());
        QTableWidgetItem* item = new QTableWidgetItem(s);
        tableWidget->setItem(i, 0, item);
    }
}

void DataView::addCalculator(Calculator *calculator)
{

    QDoubleSpinBox* spinBox = new QDoubleSpinBox();

    resultWidgets.append(spinBox);
    calculators.append(calculator);

    layout()->addWidget(spinBox);
}

void DataView::setRange(int fromIndex, int toIndex)
{
    rangeFromIndex = fromIndex;
    rangeToIndex = toIndex;

    AverageCalculator* calc = static_cast<AverageCalculator*>(calculators[0]);

    QDoubleSpinBox* spinBox = static_cast<QDoubleSpinBox*>(resultWidgets[0]);
    spinBox->setDecimals(6);
    spinBox->setValue(calc->calc(data, rangeFromIndex, rangeToIndex));
}

