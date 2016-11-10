#include "curvesettings.h"
#include <QLayout>
#include <QLabel>
#include <QStandardItemModel>
#include <QAction>

CurveSettings::CurveSettings(QWidget *parent) : QWidget(parent)
{

    QVBoxLayout* vl = new QVBoxLayout(this);


    intervalEdit = new QDoubleSpinBox();
    intervalEdit->setKeyboardTracking(false);

    xmaxEdit = new QLineEdit();

    vl->addWidget(new QLabel("Interval"));
    vl->addWidget(intervalEdit);
    vl->addWidget(xmaxEdit);
    vl->addStretch();

    connect(intervalEdit, SIGNAL(valueChanged(double)), this, SIGNAL(intervalChanged(double)));
    //connect(intervalEdit, &QDoubleSpinBox::editingFinished, this, &CurveSettings::finished);


    setMaximumWidth(200);

}

void CurveSettings::finished()
{
    double i = intervalEdit->value();
    emit intervalChanged(i);
}



