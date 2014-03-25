#-------------------------------------------------
#
# Project created by QtCreator 2014-03-23T18:53:31
#
#-------------------------------------------------
//include(../libnova/libnova.pri)
//include(../QMapControl/QMapControl.pro)

QT       += core gui
QT       += network

greaterThan(QT_MAJOR_VERSION, 4): QT += widgets

TARGET = sun
TEMPLATE = app

win32:CONFIG(release, debug|release): LIBS += -L$$OUT_PWD/../libnova/release/ -llibnova
else:win32:CONFIG(debug, debug|release): LIBS += -L$$OUT_PWD/../libnova/debug/ -llibnova
else:symbian: LIBS += -llibnova
else:unix: LIBS += -L$$OUT_PWD/../libnova/ -llibnova

INCLUDEPATH += $$PWD/../libnova
DEPENDPATH += $$PWD/../libnova


win32:CONFIG(release, debug|release): LIBS += -L$$OUT_PWD/../QMapControl/release/ -lqmapcontrol
else:win32:CONFIG(debug, debug|release): LIBS += -L$$OUT_PWD/../QMapControl/debug/ -lqmapcontrol
else:unix:!macx:!symbian: LIBS += -L$$OUT_PWD/../QMapControl/ -lqmapcontrol

INCLUDEPATH += $$PWD/../QMapControl
DEPENDPATH += $$PWD/../QMapControl

SOURCES += main.cpp\
        mainwindow.cpp

HEADERS  += mainwindow.h

FORMS    += mainwindow.ui
