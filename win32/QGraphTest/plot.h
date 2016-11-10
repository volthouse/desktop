#ifndef PLOT_H
#define PLOT_H

#include <qwt_plot.h>
#include <qwt_plot_marker.h>
#include <qwt_plot_curve.h>
#include <qwt_legend.h>
#include <qwt_point_data.h>
#include <qwt_plot_canvas.h>
#include <qwt_plot_panner.h>
#include <qwt_plot_magnifier.h>
#include <qwt_text.h>
#include <qwt_symbol.h>
#include <qwt_math.h>
#include <qwt_plot_grid.h>

#include <qwt_plot_zoomer.h>
#include <qwt_picker_machine.h>

#include <QMenu>

#include <data.h>
#include <curvetracker.h>

class Plot : public QwtPlot
{
public:
    Plot( QWidget *parent = NULL );
    void open(QString filename);
    void attachData(Data* data);
    void paste();
    void addCursor();

    void setXInterval(float interval);

    QwtPlotCurve* curve;
    QList<CurveTracker*> trackers;    

protected:
    virtual void resizeEvent( QResizeEvent * );

private:
    void populate();
    void updateGradient();
};

#endif // PLOT_H
