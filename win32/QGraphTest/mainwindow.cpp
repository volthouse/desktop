#include <QFileDialog>
#include <QDockWidget>

#include "mainwindow.h"
#include "ui_mainwindow.h"

MainWindow::MainWindow(QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::MainWindow)
{
    ui->setupUi(this);
}

MainWindow::~MainWindow()
{
    delete ui;
}

void MainWindow::on_actionOpen_triggered()
{
    QString fileName = QFileDialog::getOpenFileName(this, tr("Open File"), "/home", tr("Data (*.csv *.txt)"));

    data = new Data(fileName);


    PlotDlg* plotDlg = new PlotDlg(this);

    QDockWidget *dock = new QDockWidget(fileName, this);
    dock->setWidget(plotDlg);
    addDockWidget(Qt::TopDockWidgetArea, dock);
    plotDlg->show();
    plotDlg->plot->attachData(data);
    plotDlg->plot->replot();


    DataView* dataView = new DataView(this);
    dataView->attachData(data);

    QDockWidget *dock1 = new QDockWidget(fileName, this);
    dock1->setWidget(dataView);
    addDockWidget(Qt::TopDockWidgetArea, dock1);
    dataView->show();

    plotDlg->view = dataView;

    dataView->addCalculator(new AverageCalculator());

}

void MainWindow::on_actionPaste_triggered()
{
    PlotDlg* plotDlg = new PlotDlg(this);

    //ui->mdiArea->addSubWindow(plotDlg);

    QDockWidget *dock = new QDockWidget("", this);
    dock->setWidget(plotDlg);
    addDockWidget(Qt::TopDockWidgetArea, dock);
    plotDlg->show();
    plotDlg->plot->paste();
    plotDlg->plot->replot();
}



