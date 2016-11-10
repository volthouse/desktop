#ifndef DATA_H
#define DATA_H

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
//#include <math.h>
#include <QList>
#include <QFile>


class Data: public QwtSeriesData<QPointF>
{
private:
    QList<QPointF> data;

public:

    float yMin = 10000;
    float yMax = -10000;
    float xMin = 0;
    float xMax = 0;

    Data()
    {

    }

    Data(QString filename)
    {
        QFile file(filename);

        if(file.open(QFile::ReadOnly)) {
            QLocale l;
            int i = 0;


            while(!file.atEnd()) {
                QByteArray line = file.readLine();
                float y = l.toFloat(line);

                if(y < yMin) yMin = y;
                if(y > yMax) yMax = y;


                QPointF p(i, y);
                data.append(p);
                i++;
            }

           xMax = data.count();
        }
    }

    Data(QList<float>* values)
    {
        for(int i = 0; i < values->count(); i++) {
            float y = values->at(i);

            if(y < yMin) yMin = y;
            if(y > yMax) yMax = y;


            QPointF p(i, y);
            data.append(p);
        }

        xMax = data.count();
    }

    QRectF boundingRect() const
    {
        return qwtBoundingRect( *this );;
    }


    size_t size() const
    {
        return data.size();
    }

    QPointF sample( size_t index ) const
    {
        return data[static_cast<int>(index)];
    }

    void setInterval(const float interval)
    {
        //const
        for(int i = 0; i < data.count(); i++) {
            data[i].setX(interval * i);
        }

        xMax = interval * data.count();
    }


};

#endif // DATA_H
