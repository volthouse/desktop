#ifndef PLOTDLG_H
#define PLOTDLG_H

#include <QWidget>
#include <QMenu>
#include <QToolBar>
#include <QDockWidget>
#include <QDialog>

#include "curvesettings.h"
#include "plot.h"
#include "averagecalculator.h"
#include "dataview.h"

class PlotDlg : public QDialog
{
    Q_OBJECT

private:
    CurveSettings* curveSettings;
    QToolBar* toolBar;

public:
    explicit PlotDlg(QWidget *parent = 0);

    Plot* plot;

    QMenu* popupMenu;
    QAction* addTrackerAction;
    QAction* formatXAxisAction;

    AverageCalculator avgCalc;
    DataView* view;

public slots:
    void addTracker();
    void formatXAxis();
    void ShowContextMenu(const QPoint&);
    void showCurveSettings();
    void intervalChanged(double);
};

#endif // PLOTDLG_H
