#include "plot.h"
#include "curvetracker.h"
#include <QFile>
#include <QClipboard>
#include <QApplication>

Plot::Plot( QWidget *parent ):
    QwtPlot( parent )
{
    //setAutoFillBackground( true );
    //setPalette( QPalette( QColor( 165, 193, 228 ) ) );
    //updateGradient();

    //setTitle( "A Simple QwtPlot Demonstration" );
    //insertLegend( new QwtLegend(), QwtPlot::BottomLegend );

    // axes
    setAxisTitle( xBottom, "x -->" );
    setAxisScale( xBottom, 0.0, 360.0 );

    setAxisTitle( yLeft, "y -->" );
    setAxisScale( yLeft, -10.0, 10.0 );


    // canvas
    QwtPlotCanvas *canvas = new QwtPlotCanvas();
    //canvas->setLineWidth( 1 );
    canvas->setFrameStyle( QFrame::StyledPanel  );
    //canvas->setBorderRadius( 15 );

    QwtPlotGrid* grid = new QwtPlotGrid();
    grid->setPen(Qt::black, 0.1, Qt::DashLine);
    grid->attach(this);

    QPalette canvasPalette( Qt::white );
    canvasPalette.setColor( QPalette::Foreground, QColor( 133, 190, 232 ) );
    canvas->setPalette( canvasPalette );

    setCanvas( canvas );

    // panning with the left mouse button
    QwtPlotPanner* panner = new QwtPlotPanner( canvas );
    panner->setMouseButton(Qt::LeftButton, Qt::ControlModifier);

    // zoom in/out with the wheel
    QwtPlotMagnifier* magnifier = new QwtPlotMagnifier( canvas );
    magnifier->setAxisEnabled(xBottom, false);


    QwtPlotZoomer* zoomer = new QwtPlotZoomer(canvas);
    zoomer->setKeyPattern( QwtEventPattern::KeyRedo, Qt::Key_I, Qt::ShiftModifier );
    zoomer->setKeyPattern( QwtEventPattern::KeyUndo, Qt::Key_O, Qt::ShiftModifier );
    zoomer->setKeyPattern( QwtEventPattern::KeyHome, Qt::Key_Home );

    zoomer->setMousePattern(QwtEventPattern::MouseSelect1, Qt::LeftButton, Qt::ShiftModifier);
    zoomer->setMousePattern(QwtEventPattern::MouseSelect2, Qt::RightButton, Qt::ShiftModifier);




    canvas->setFocusPolicy(Qt::StrongFocus);
    canvas->setFocusIndicator(QwtPlotCanvas::ItemFocusIndicator);
    canvas->setFocus();
    //canvas->setFrameShadow(QwtPlot::Plain);
    //canvas->setCursor(Qt::arrowCursor);
    canvas->setLineWidth(0);
    //canvas->setPaintAttribute(QwtPlotCanvas::PaintCached, false);
    //canvas->setPaintAttribute(QwtPlotCanvas::PaintPacked, false);

    //populate();    
}

void Plot::open(QString filename)
{

    this->detachItems();

    // Insert new curves
    curve = new QwtPlotCurve( "y = sin(x)" );
    curve->setRenderHint( QwtPlotItem::RenderAntialiased );
    curve->setLegendAttribute( QwtPlotCurve::LegendShowLine, true );
    curve->setPen( Qt::red );
    curve->attach( this );

    Data* data = new Data(filename);

    curve->setData(data);

    setAxisScale( yLeft, data->yMin, data->yMax);
    setAxisScale( xBottom, data->xMin, data->xMax);


    QwtPlotGrid* grid = new QwtPlotGrid();
    grid->setPen(Qt::black, 0.1, Qt::DashLine);
    grid->attach(this);

}

void Plot::attachData(Data *data)
{
    this->detachItems();

    // Insert new curves
    curve = new QwtPlotCurve( "y = sin(x)" );
    curve->setRenderHint( QwtPlotItem::RenderAntialiased );
    curve->setLegendAttribute( QwtPlotCurve::LegendShowLine, true );
    curve->setPen( Qt::red );
    curve->attach( this );


    curve->setData(data);

    setAxisScale( yLeft, data->yMin, data->yMax);
    setAxisScale( xBottom, data->xMin, data->xMax);


    QwtPlotGrid* grid = new QwtPlotGrid();
    grid->setPen(Qt::black, 0.1, Qt::DashLine);
    grid->attach(this);
}

void Plot::paste()
{
    QLocale l;
    QClipboard *clipboard = QApplication::clipboard();
    QString text = clipboard->text();

    if(!text.isEmpty()) {
        QList<QString> items = text.split('\n');
        QList<float> values;
        for(int i = 0; i < items.count(); i++) {
            values.append(l.toFloat(items[i]));
        }


        this->detachItems();

        // Insert new curves
        curve = new QwtPlotCurve( "y = sin(x)" );
        curve->setRenderHint( QwtPlotItem::RenderAntialiased );
        curve->setLegendAttribute( QwtPlotCurve::LegendShowLine, true );
        curve->setPen( Qt::red );
        curve->attach( this );

        Data* data = new Data(&values);
        curve->setData(data);

        setAxisScale( yLeft, data->yMin, data->yMax);
        setAxisScale( xBottom, data->xMin, data->xMax);


        QwtPlotGrid* grid = new QwtPlotGrid();
        grid->setPen(Qt::black, 0.1, Qt::DashLine);
        grid->attach(this);
    }
}

void Plot::addCursor()
{


    CurveTracker* tracker = new CurveTracker( canvas() );
    //QPen pen(Qt::green, 3, Qt::DashDotLine, Qt::RoundCap, Qt::RoundJoin);
    //tracker2->setRubberBandPen(pen );
    tracker->setRubberBandPen( QPen( "MediumOrchid" ) );
    QPointF p(100,100);

    tracker->activate(p);

    trackers.append(tracker);;
}

void Plot::setXInterval(float interval)
{
    Data* data = static_cast<Data*>(curve->data());
    data->setInterval(interval);
    setAxisScale( xBottom, data->xMin, data->xMax);
}

void Plot::populate()
{
    // Insert new curves
    QwtPlotCurve *cSin = new QwtPlotCurve( "y = sin(x)" );
    cSin->setRenderHint( QwtPlotItem::RenderAntialiased );
    cSin->setLegendAttribute( QwtPlotCurve::LegendShowLine, true );
    cSin->setPen( Qt::red );
    cSin->attach( this );

    cSin->setData( new Data() );



    CurveTracker* tracker = new CurveTracker( canvas() );

    // for the demo we want the tracker to be active without
    // having to click on the canvas
    tracker->setStateMachine( new QwtPickerTrackerMachine() );
    tracker->setRubberBandPen( QPen( "MediumOrchid" ) );

}

void Plot::updateGradient()
{
    QPalette pal = palette();

    const QColor buttonColor = pal.color( QPalette::Button );

    QLinearGradient gradient( rect().topLeft(), rect().bottomLeft() );
    gradient.setColorAt( 0.0, Qt::white );
    gradient.setColorAt( 0.7, buttonColor );
    gradient.setColorAt( 1.0, buttonColor );

    pal.setBrush( QPalette::Window, gradient );
    setPalette( pal );
}

void Plot::resizeEvent( QResizeEvent *event )
{
    QwtPlot::resizeEvent( event );

    // Qt 4.7.1: QGradient::StretchToDeviceMode is buggy on X11
    updateGradient();
}


