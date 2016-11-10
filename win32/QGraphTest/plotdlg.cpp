#include "plotdlg.h"
#include <QHBoxLayout>
#include <QComboBox>

#include "mainwindow.h"
#include "trackerobserver.h"

PlotDlg::PlotDlg(QWidget *parent) : QDialog(parent)
{

    toolBar = new QToolBar("Main", this);
    toolBar->setIconSize( QSize(22,22) );

    QPixmap newpix("/resources/Edit.png");


    plot = new Plot(this);

    addTrackerAction = new QAction(tr("New &Tracker"), this);
    addTrackerAction->setStatusTip(tr("Create a new tracker"));
    connect(addTrackerAction, &QAction::triggered, this, &PlotDlg::addTracker);

    formatXAxisAction = new QAction(tr("Format &X-Axis"), this);
    formatXAxisAction->setStatusTip(tr("Format X-Axis"));
    connect(formatXAxisAction, &QAction::triggered, this, &PlotDlg::formatXAxis);

    popupMenu = new QMenu(this);
    popupMenu->addAction(addTrackerAction);
    popupMenu->addAction(formatXAxisAction);


    setContextMenuPolicy(Qt::CustomContextMenu);
    connect(this, SIGNAL(customContextMenuRequested(const QPoint&)),
        this, SLOT(ShowContextMenu(const QPoint&)));


    QAction* propAction = toolBar->addAction(QIcon(newpix), "Properties");
    connect(propAction, &QAction::triggered, this, &PlotDlg::formatXAxis);


    curveSettings = new CurveSettings();
    connect(curveSettings, &CurveSettings::intervalChanged, this, &PlotDlg::intervalChanged);

    QVBoxLayout* vb = new QVBoxLayout(this);
    vb->addWidget(toolBar);
    vb->addWidget(plot);
    vb->addWidget(curveSettings);

    curveSettings->hide();
}

void PlotDlg::addTracker()
{
    plot->addCursor();

    if(plot->trackers.count() == 2) {
        //avgCalc.setTracker(plot->trackers[0], plot->trackers[1]);

        //MainWindow* wnd = static_cast<MainWindow*>(parentWidget());

        TrackerObserver* o = new TrackerObserver(view, plot->trackers[0], plot->trackers[1]);

    }

    //MainWindow::view

}

void PlotDlg::formatXAxis()
{
    //plot->setXInterval(1.0f / 30000.0f);
    //plot->replot();

    curveSettings->show();

}

void PlotDlg::ShowContextMenu(const QPoint &pos)
{
    QPoint globalPos = mapToGlobal(pos);
    popupMenu->popup(globalPos);
}

void PlotDlg::showCurveSettings()
{

}

void PlotDlg::intervalChanged(double interval)
{
    plot->setXInterval(interval);
    plot->replot();
}
