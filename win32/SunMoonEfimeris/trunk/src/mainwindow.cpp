#include "mainwindow.h"
#include "ui_mainwindow.h"

#include <stdio.h>


MainWindow::MainWindow(QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::MainWindow)
{
    ui->setupUi(this);


    // create MapControl
    mc = new MapControl(QSize(638,370));
    mc->showScale(true);
    mc->setSizePolicy(QSizePolicy::Maximum,
                      QSizePolicy::Maximum);
    ui->verticalLayout->addWidget(mc);

    mapadapter = new GoogleMapAdapter();
    //MapAdapter* mapadapter_overlay = new YahooMapAdapter("us.maps3.yimg.com", "/aerial.maps.yimg.com/png?v=2.2&t=h&s=256&x=%2&y=%3&z=%1");


    // create a layer with the mapadapter and type MapLayer
    l = new MapLayer("Custom Layer", mapadapter);
//    overlay = new MapLayer("Overlay", mapadapter_overlay);
//    overlay->setVisible(false);

    mc->addLayer(l);
   // mc->addLayer(overlay);

    notes = new GeometryLayer("Sun", mapadapter);
    mc->addLayer(notes);


    QPen* pen = new QPen(QColor(0,0,255,100));
    pen->setWidth(5);


    sunHeading = new Vector(8.85,51.46, 1000, 45, "SunArrow", Vector::Middle, pen);
    sunRise = new Vector(8.85,51.46, 1000, 45, "SunRiseArrow", Vector::Middle, pen);
    sunSet = new Vector(8.85,51.46, 1000, 45, "SunSetArrow", Vector::Middle, pen);

    QList<Point*> points;
    points << sunHeading;
    points << sunRise;
    points << sunSet;

    LineString* sunArrows = new LineString(points, "", pen);

    notes->addGeometry(sunArrows);



    mc->setView(QPointF(8.85,51.46));
    mc->setZoom(10);

    ui->dateTimeEdit->setDateTime(QDateTime::currentDateTime());
    ui->latEdit->setText("51,46");
    ui->lngEdit->setText("8,85");

    addZoomButtons();
}

MainWindow::~MainWindow()
{
    delete ui;
}


void MainWindow::on_calcButton_clicked()
{
    struct ln_equ_posn equ;
    struct ln_rst_time rst;
    struct ln_zonedate rise, set, transit;
    struct ln_lnlat_posn observer;

    struct ln_hrz_posn hpos;

    double JD;

    observer.lat = ui->latEdit->text().toFloat();
    observer.lng = ui->lngEdit->text().toFloat();

    ln_date date;
    date.years = ui->dateTimeEdit->dateTime().date().year();
    date.months = ui->dateTimeEdit->dateTime().date().month();
    date.days = ui->dateTimeEdit->dateTime().date().day();
    date.hours = ui->dateTimeEdit->dateTime().time().hour();
    date.minutes = ui->dateTimeEdit->dateTime().time().minute();
    date.seconds = ui->dateTimeEdit->dateTime().time().second();

    JD = ln_get_julian_day(&date);

    /* ra, dec */
    ln_get_solar_equ_coords (JD, &equ);


    ln_get_hrz_from_equ(&equ, &observer, JD, &hpos);

    double a = ln_range_degrees(hpos.az - 180);

    QString s;
    s.sprintf("Azimut: %0.3f", a);
    ui->listWidget->addItem(s);
    s.sprintf("Evaluation: %0.3f", hpos.alt);
    ui->listWidget->addItem(s);

    /* rise, set and transit */
    if (ln_get_solar_rst (JD, &observer, &rst) == 1) {
        ui->listWidget->addItem(QString("Zirkumpolar"));
    } else {
        ln_get_local_date (rst.rise, &rise);
        ln_get_local_date (rst.transit, &transit);
        ln_get_local_date (rst.set, &set);
        s.sprintf("Aufgang: %02d:%02d:%02d", rise.hours, rise.minutes, (int)rise.seconds);
        ui->listWidget->addItem(s);
        s.sprintf("Transit: %02d:%02d:%02d", transit.hours, transit.minutes, (int)transit.seconds);
        ui->listWidget->addItem(s);
        s.sprintf("Untergang: %02d:%02d:%02d", set.hours, set.minutes, (int)set.seconds);
        ui->listWidget->addItem(s);

    }

    sunHeading->setVisible(false);

    setSunVectors(&rise, &observer, sunRise);
    setSunVectors(&set, &observer, sunSet);

    mc->setView(QPointF(observer.lng, observer.lat));

    //overlay->addGeometry();
}

void MainWindow::on_dateTimeEdit_dateTimeChanged(const QDateTime &date)
{
    struct ln_equ_posn equ;
    struct ln_lnlat_posn observer;

    struct ln_hrz_posn hpos;

    double JD;

    observer.lat = ui->latEdit->text().toFloat();
    observer.lng = ui->lngEdit->text().toFloat();

    ln_date novaDate;
    novaDate.years = date.date().year();
    novaDate.months = date.date().month();
    novaDate.days = date.date().day();
    novaDate.hours = date.time().hour();
    novaDate.minutes = date.time().minute();
    novaDate.seconds = date.time().second();

    JD = ln_get_julian_day(&novaDate);

    /* ra, dec */
    ln_get_solar_equ_coords (JD, &equ);


    ln_get_hrz_from_equ(&equ, &observer, JD, &hpos);

    double a = ln_range_degrees(hpos.az - 180);

    sunHeading->setHeading(a);

    mc->updateRequestNew();

}

void MainWindow::setSunVectors(ln_zonedate* date, ln_lnlat_posn* observer, Vector* vector)
{
    struct ln_equ_posn equ;
    struct ln_hrz_posn hpos;

    double JD;

    ln_date lnDate;
    lnDate.years = date->years;
    lnDate.months = date->months;
    lnDate.days = date->days;
    lnDate.hours = date->hours;
    lnDate.minutes = date->minutes;
    lnDate.seconds = date->seconds;

    JD = ln_get_julian_day(&lnDate);

    /* ra, dec */
    ln_get_solar_equ_coords (JD, &equ);


    ln_get_hrz_from_equ(&equ, observer, JD, &hpos);

    double a = ln_range_degrees(hpos.az - 180);
    vector->setHeading(a);
}

void MainWindow::addZoomButtons()
{
    // create buttons as controls for zoom
    QPushButton* zoomin = new QPushButton("+");
    QPushButton* zoomout = new QPushButton("-");
    zoomin->setMaximumWidth(50);
    zoomout->setMaximumWidth(50);

    connect(zoomin, SIGNAL(clicked(bool)),
              mc, SLOT(zoomIn()));
    connect(zoomout, SIGNAL(clicked(bool)),
              mc, SLOT(zoomOut()));

    // add zoom buttons to the layout of the MapControl
    QVBoxLayout* innerlayout = new QVBoxLayout;
    innerlayout->addWidget(zoomin);
    innerlayout->addWidget(zoomout);
    mc->setLayout(innerlayout);
}
