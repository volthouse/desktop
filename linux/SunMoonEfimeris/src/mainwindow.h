#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QMainWindow>
#include <mapcontrol.h>
#include <osmmapadapter.h>
#include <yahoomapadapter.h>
#include <googlemapadapter.h>
#include <maplayer.h>
#include <geometrylayer.h>
#include <linestring.h>
#include <imagepoint.h>
#include <vector.h>

using namespace qmapcontrol;

namespace Ui {
class MainWindow;
}

class MainWindow : public QMainWindow
{
    Q_OBJECT
    
public:
    explicit MainWindow(QWidget *parent = 0);
    ~MainWindow();
    
private slots:
    void on_calcButton_clicked();

    void on_dateTimeEdit_dateTimeChanged(const QDateTime &date);

private:
    Ui::MainWindow *ui;
    MapControl* mc;
    MapAdapter* mapadapter;
    Layer* l;
    Layer* overlay;
    Layer* notes;
    Vector* sunHeading;
    Vector* sunRise;
    Vector* sunSet;
};

#endif // MAINWINDOW_H
