#ifndef CURVESETTINGS_H
#define CURVESETTINGS_H

#include <QWidget>
#include <QLineEdit>
#include <QDoubleSpinBox>
#include <QComboBox>
#include <QStackedWidget>

class CurveSettings : public QWidget
{
    Q_OBJECT
public:
    explicit CurveSettings(QWidget *parent = 0);

signals:
    void intervalChanged(double);


public slots:
    void finished();

private:
    QDoubleSpinBox* intervalEdit;
    QLineEdit* xmaxEdit;



};

#endif // CURVESETTINGS_H
