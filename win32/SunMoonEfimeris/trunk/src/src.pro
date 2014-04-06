#-------------------------------------------------
#
# Project created by QtCreator 2014-03-23T18:53:31
#
#-------------------------------------------------

#CONFIG += static
QT       += core gui
QT       += network

greaterThan(QT_MAJOR_VERSION, 4): QT += widgets

TARGET = sun
TEMPLATE = app

win32:CONFIG(release, debug|release): LIBS += -L$$OUT_PWD/../libnova/release/ -llibnova
else:win32:CONFIG(debug, debug|release): LIBS += -L$$OUT_PWD/../libnova/debug/ -llibnova
else:unix: LIBS += -L$$OUT_PWD/../libnova/ -llibnova

INCLUDEPATH += $$PWD/../libnova
DEPENDPATH += $$PWD/../libnova


win32:CONFIG(release, debug|release): LIBS += -L$$OUT_PWD/../QMapControl/release/ -lqmapcontrol0
else:win32:CONFIG(debug, debug|release): LIBS += -L$$OUT_PWD/../QMapControl/debug/ -lqmapcontrol0
else:unix:!macx:!symbian: LIBS += -L$$OUT_PWD/../QMapControl/ -lqmapcontrol

INCLUDEPATH += $$PWD/../QMapControl
DEPENDPATH += $$PWD/../QMapControl

SOURCES += main.cpp\
        mainwindow.cpp

HEADERS  += mainwindow.h

FORMS    += mainwindow.ui
