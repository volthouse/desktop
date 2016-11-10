#-------------------------------------------------
#
# Project created by QtCreator 2016-10-19T14:00:22
#
#-------------------------------------------------

QT       += core gui

greaterThan(QT_MAJOR_VERSION, 4): QT += widgets


TARGET = QPlotTest
TEMPLATE = app


#win32:CONFIG(release, debug|release): LIBS += -L$$PWD/D:/Qt/Qt5.7.0/5.7/msvc2013_64/lib/ -lqwt
#else:win32:CONFIG(debug, debug|release): LIBS += -L$$PWD/D:/Qt/Qt5.7.0/5.7/msvc2013_64/lib/ -lqwtd

#INCLUDEPATH += $$PWD/../../QKeithley/qwt-6.1/src
#DEPENDPATH += $$PWD/../../QKeithley/qwt-6.1/src

#include ( ${QWT_ROOT}/features/qwt.prf )
include (D:/Qt/qwt-6.1.3/features/qwt.prf )


SOURCES += main.cpp\
        mainwindow.cpp \
    curvetracker.cpp \
    plot.cpp \
    plotdlg.cpp \
    data.cpp \
    curvesettings.cpp \
    averagecalculator.cpp \
    dataview.cpp \
    calculator.cpp \
    trackerobserver.cpp

HEADERS  += mainwindow.h \
    curvetracker.h \
    plot.h \
    plotdlg.h \
    data.h \
    curvesettings.h \
    averagecalculator.h \
    dataview.h \
    calculator.h \
    trackerobserver.h

FORMS    += mainwindow.ui

RESOURCES += \
    res.qrc

DISTFILES += \
    resources/Edit.png


