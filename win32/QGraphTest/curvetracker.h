#ifndef _CURVE_TRACKER_H_
#define _CURVE_TRACKER_H_

#include <qwt_plot_picker.h>
#include "qwt_picker_machine.h"
#include <qlist.h>
#include <qevent.h>
#include <QPointF>

class QwtPlotCurve;

class CurveTracker: public QwtPlotPicker
{

public:
    CurveTracker( QWidget * );

    virtual void transition( const QEvent* event);

    void activate(QPointF& p);

public:
    float currentX;

protected:
    virtual QwtText trackerTextF( const QPointF & ) const;
    virtual QRect trackerRect( const QFont & ) const;
    virtual void move( const QPoint & );

private:
    QString curveInfoAt( const QwtPlotCurve *, const QPointF & ) const;
    QLineF curveLineAt( const QwtPlotCurve *, double x ) const;
};

class QwtPickerTrackerMachineEx: public QwtPickerTrackerMachine
{
public:
    QwtPickerTrackerMachineEx();

    virtual QList<Command> transition(
        const QwtEventPattern &, const QEvent * );
};

#endif
