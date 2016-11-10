#ifndef TRACKEROBSERVER_H
#define TRACKEROBSERVER_H

#include <QObject>

#include "dataview.h"
#include "curvetracker.h"

class TrackerObserver : public QObject
{
    Q_OBJECT

private:
    DataView *view;
    CurveTracker* t1;
    CurveTracker* t2;

private slots:
    void trackerMoved(const QPointF &pos);

public:
    explicit TrackerObserver(DataView* view, CurveTracker* t1, CurveTracker* t2);

signals:

public slots:
};

#endif // TRACKEROBSERVER_H
