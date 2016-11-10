/********************************************************************************
** Form generated from reading UI file 'plotdialog.ui'
**
** Created by: Qt User Interface Compiler version 5.7.0
**
** WARNING! All changes made in this file will be lost when recompiling UI file!
********************************************************************************/

#ifndef UI_PLOTDIALOG_H
#define UI_PLOTDIALOG_H

#include <QtCore/QVariant>
#include <QtWidgets/QAction>
#include <QtWidgets/QApplication>
#include <QtWidgets/QButtonGroup>
#include <QtWidgets/QDialog>
#include <QtWidgets/QHeaderView>

QT_BEGIN_NAMESPACE

class Ui_PlotDialog
{
public:

    void setupUi(QDialog *PlotDialog)
    {
        if (PlotDialog->objectName().isEmpty())
            PlotDialog->setObjectName(QStringLiteral("PlotDialog"));
        PlotDialog->resize(400, 300);

        retranslateUi(PlotDialog);

        QMetaObject::connectSlotsByName(PlotDialog);
    } // setupUi

    void retranslateUi(QDialog *PlotDialog)
    {
        PlotDialog->setWindowTitle(QApplication::translate("PlotDialog", "Dialog", 0));
    } // retranslateUi

};

namespace Ui {
    class PlotDialog: public Ui_PlotDialog {};
} // namespace Ui

QT_END_NAMESPACE

#endif // UI_PLOTDIALOG_H
