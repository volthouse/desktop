#include "trackerobserver.h"

/*
TrackerObserver::TrackerObserver(QObject *parent) : QObject(parent)
{

}
*/

TrackerObserver::TrackerObserver(DataView *view, CurveTracker *t1, CurveTracker *t2) : QObject()
{
    this->view = view;
    this->t1 = t1;
    this->t2 = t2;

    connect(t1, SIGNAL(moved(QPointF)), this, SLOT(trackerMoved(QPointF)));
    connect(t2, SIGNAL(moved(QPointF)), this, SLOT(trackerMoved(QPointF)));
}

void TrackerObserver::trackerMoved(const QPointF &pos)
{
    double x1 = qMin(t1->currentX, t2->currentX);
    double x2 = qMax(t1->currentX, t2->currentX);

    view->setRange(qCeil(x1), qCeil(x2));
}


