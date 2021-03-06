/*
*
* This file is part of QMapControl,
* an open-source cross-platform map widget
*
* Copyright (C) 2010 Jeffery MacEachern
* Based on CirclePoint code by Kai Winter
*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU Lesser General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* This program is distributed in the hope that it will `be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Lesser General Public License for more details.
*
* You should have received a copy of the GNU Lesser General Public License
* along with QMapControl. If not, see <http://www.gnu.org/licenses/>.
*
* Contact e-mail: kaiwinter@gmx.de
* Program URL   : http://qmapcontrol.sourceforge.net/
*
*/

#include "vector.h"
namespace qmapcontrol
{
    Vector::Vector(qreal x, qreal y, int sideLength, qreal heading, QString name, qmapcontrol::Point::Alignment alignment, QPen* pen)
        : Point(x, y, name, alignment)
    {
        size = QSize(sideLength, sideLength);
        h = heading;
        mypen = pen;
        mypixmap = QPixmap(sideLength, sideLength);
        drawArrow();
    }

    Vector::~Vector()
    {
    }
   
    void Vector::setHeading(qreal heading)
    {
        h = heading;
        drawArrow();
    }

    qreal Vector::getHeading() const
    {
        return h;
    }
    
    void Vector::setPen(QPen* pen)
    {
        mypen = pen;
        drawArrow();
    }

    void Vector::drawArrow()
    {
        mypixmap = QPixmap(size);
        mypixmap.fill(Qt::transparent);
        QPainter painter(&mypixmap);
//#if !defined Q_WS_MAEMO_5  //FIXME Maemo has a bug - it will antialias our point out of existence
        painter.setRenderHints(QPainter::Antialiasing|QPainter::HighQualityAntialiasing);
//#endif

        if(mypen)
        {
            painter.setPen(*mypen);
            painter.setBrush(QBrush(mypen->color()));
        }
        else
        {
            painter.setBrush(QBrush(painter.pen().color()));
        }

        painter.setWindow(-(size.width() / 2), -(size.height() / 2), size.width(), size.height());
        QTransform transform;
        transform.rotate(-h);
        transform.scale(0.4, 0.75);
        painter.setWorldTransform(transform);

        QPolygon arrow;
        arrow << QPoint(0, -(size.height() / 2));
        //arrow << QPoint(-(size.width() / 2), +(size.height() / 2));
        arrow << QPoint(0, 0);
        //arrow << QPoint(+(size.width() / 2), +(size.height() / 2));

        painter.drawPolygon(arrow);
    }

}
