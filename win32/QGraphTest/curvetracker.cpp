#include "curvetracker.h"
#include <qwt_picker_machine.h>
#include <qwt_series_data.h>
#include <qwt_plot.h>
#include <qwt_plot_curve.h>
#include <qwt_plot_marker.h>
#include <qwt_picker.h>

struct compareX
{
    inline bool operator()( const double x, const QPointF &pos ) const
    {
        return ( x < pos.x() );
    }
};

CurveTracker::CurveTracker( QWidget *canvas ):
    QwtPlotPicker( canvas )
{
    setTrackerMode( QwtPlotPicker::ActiveOnly );
    setRubberBand( VLineRubberBand );    
    setStateMachine( new QwtPickerTrackerMachineEx() );
    setRubberBandPen( QPen( "MediumOrchid" ) );

    currentX = 0;
}

void CurveTracker::transition(const QEvent* event)
{
    QEvent::Type t = event->type();

    switch(event->type()) {
        case QEvent::MouseButtonPress:
        {
            const QMouseEvent *me =
                static_cast< const QMouseEvent * >( event );

            if(me->button() == Qt::RightButton)
                return;


            if(this->pickedPoints().count() > 0) {
                QPoint p = pickedPoints().at(0);
                int x1 = p.x();
                int x2 = me->pos().x();
                if(abs(x1 - x2) < 5)
                    QwtPlotPicker::transition(event);
            }



            break;
        }

        default: {
            QwtPlotPicker::transition(event);
        }
    }
}

void CurveTracker::activate(QPointF &p)
{

    this->remove();
    this->end();
    this->begin();
    this->append(p.toPoint());
    this->move(p.toPoint());;


    QPoint pp = trackerPosition();

    //setMouseTracking(true);
}

QRect CurveTracker::trackerRect( const QFont &font ) const
{
    QRect r = QwtPlotPicker::trackerRect( font );
    
    // align r to the first curve

    const QwtPlotItemList curves = plot()->itemList( QwtPlotItem::Rtti_PlotCurve );
    if ( curves.size() > 0 )
    {
        QPointF pos = invTransform( trackerPosition() );

        const QLineF line = curveLineAt(    
            static_cast<const QwtPlotCurve *>( curves[0] ), pos.x() );
        if ( !line.isNull() )
        {
            const double curveY = line.pointAt(
                ( pos.x() - line.p1().x() ) / line.dx() ).y();

            pos.setY( curveY );
            pos = transform( pos );

            r.moveBottom( pos.y() );            
        }
    }

    return r;
}

void CurveTracker::move(const QPoint &pos)
{
    currentX = invTransform( pos ).x();
    QwtPlotPicker::move(pos);
}

QwtText CurveTracker::trackerTextF( const QPointF &pos ) const
{
    QwtText trackerText;

    trackerText.setColor( Qt::black );

    QColor c( "#333333" );
    trackerText.setBorderPen( QPen( c, 2 ) );
    c.setAlpha( 200 );
    trackerText.setBackgroundBrush( c );

    QString info;

    const QwtPlotItemList curves = 
        plot()->itemList( QwtPlotItem::Rtti_PlotCurve );

    for ( int i = 0; i < curves.size(); i++ )
    {
        const QString curveInfo = curveInfoAt( 
            static_cast<const QwtPlotCurve *>( curves[i] ), pos );

        if ( !curveInfo.isEmpty() )
        {
            if ( !info.isEmpty() )
                info += "<br>";

            info += curveInfo;
        }
    }

    trackerText.setText( info );
    return trackerText;
}

QString CurveTracker::curveInfoAt( 
    const QwtPlotCurve *curve, const QPointF &pos ) const
{
    const QLineF line = curveLineAt( curve, pos.x() );
    if ( line.isNull() )
        return QString::null;

    QPointF p = line.pointAt(( pos.x() - line.p1().x() ) / line.dx());

    QString info( "<font color=""%1"">%2 / %3</font>" );
    return info.arg( curve->pen().color().name() ).arg( p.y() ).arg(p.x());
}

QLineF CurveTracker::curveLineAt( 
    const QwtPlotCurve *curve, double x ) const
{
    QLineF line;

    if ( curve->dataSize() >= 2 )
    {
        const QRectF br = curve->boundingRect();
        if ( ( br.width() > 0 ) && ( x >= br.left() ) && ( x <= br.right() ) )
        {
            int index = qwtUpperSampleIndex<QPointF>( 
                *curve->data(), x, compareX() );

            if ( index == -1 && 
                x == curve->sample( curve->dataSize() - 1 ).x() )
            {
                // the last sample is excluded from qwtUpperSampleIndex
                index = curve->dataSize() - 1;
            }

            if ( index > 0 )
            {
                line.setP1( curve->sample( index - 1 ) );
                line.setP2( curve->sample( index ) );
            }
        }
    }
    
    return line;
}





QwtPickerTrackerMachineEx::QwtPickerTrackerMachineEx():
    QwtPickerTrackerMachine(  )
{
}

//! Transition
QList<QwtPickerMachine::Command> QwtPickerTrackerMachineEx::transition(
    const QwtEventPattern &, const QEvent *e )
{
    QList<QwtPickerMachine::Command> cmdList;

    QEvent::Type t = e->type();

    switch ( e->type() )
    {
        case QEvent::MouseButtonPress:
        {
            if ( state() == 0 )
            {
                cmdList += Remove;
                cmdList += End;
                cmdList += Begin;
                cmdList += Append;
                setState( 1 );
            }
            break;
        }


        case QEvent::MouseButtonRelease:
        {
            setState( 0 );
            break;
        }

        case QEvent::MouseMove:
        {
            if ( state() == 1 )
            {
                cmdList += Move;
            }
            break;
        }

    }

    return cmdList;
}



