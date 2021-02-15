from PyQt5 import QtGui, QtCore
from PyQt5.QtWidgets import QWidget, QApplication, QLabel, QHBoxLayout, QListWidget, QVBoxLayout, QListWidgetItem,QGridLayout
from PyQt5.QtGui import QPixmap

import sys
import cv2
from PyQt5.QtCore import pyqtSignal, pyqtSlot, Qt, QThread, QDir
import numpy as np
import ctypes
import time
import threading

class VideoThread(QThread):
    change_pixmap_signal = pyqtSignal(np.ndarray)
    path = ""

    def __init__(self):
        super().__init__()
        self._run_flag = True
        print(self.path)

    def run(self):
        # capture from web cam
        #
        print("started thread")
        try:
            print(self._run_flag)
            self.cap = cv2.VideoCapture(self.path)
        except Exception as er:
            print(er)
        print("FLAG:", self._run_flag)
        while self._run_flag:

            ret, cv_img = self.cap.read()
            if ret:
                self.change_pixmap_signal.emit(cv_img)
        # shut down capture system
        self.cap.release()

    def stop(self):
        """Sets run flag to False and waits for thread to finish"""
        self._run_flag = False
        self.wait()


    def get_id(self):

        # returns id of the respective thread
        if hasattr(self, '_thread_id'):
            return self._thread_id
        for id, thread in threading._active.items():
            if thread is self:
                return id

    def raise_exception(self):
        thread_id = self.get_id()
        res = ctypes.pythonapi.PyThreadState_SetAsyncExc(thread_id,
                                                         ctypes.py_object(SystemExit))
        if res > 1:
            ctypes.pythonapi.PyThreadState_SetAsyncExc(thread_id, 0)
            print('Exception raise failure')

class App(QWidget):
    keyPressed = QtCore.pyqtSignal(QtCore.QEvent)
    def __init__(self):
        super().__init__()
        self.setWindowTitle("Qt live label demo")
        self.disply_width = 900
        self.display_height = 700
        # create the label that holds the image
        self.image_label = QLabel(self)
        self.image_label.resize(self.disply_width, self.display_height)

        # create a vertical box layout and add the two labels
        hbox = QHBoxLayout()
        sublayout = QGridLayout()

        listwidget = QListWidget()

        directory = QDir("C:\Personal\dataset\Washinghands")
        directory.setFilter(QDir.Files | QDir.Hidden | QDir.NoSymLinks)
        directory.setSorting(QDir.Size | QDir.Reversed)

        for entry in directory.entryInfoList():
            item = QListWidgetItem(f"{entry.fileName()}")
            listwidget.addItem(item)

        listwidget.itemClicked.connect(self.clicked)
        listwidget.setFixedSize(300, 200)

        sublayout.addWidget(listwidget, 0, 0, 0, 0)
        listwidget2 = QListWidget()
        listwidget2.addItem("Test2")
        listwidget2.addItem("Test3")
        listwidget2.addItem("Test4")
        listwidget2.setFixedSize(200, 100)
        hbox.addLayout(sublayout)
        hbox.addWidget(self.image_label)
        hbox.addWidget(listwidget2)
        self.setLayout(hbox)

        # create the video capture thread
        self.thread = VideoThread()
        self.thread.change_pixmap_signal.connect(self.update_image)
        self.thread.path = "C:\Personal\dataset\Washinghands\\Other_SC01_150902_1528_C104_1da146c17071462b81dc721c106f60c9_1 (48).m4v"
        # start the thread
        self.thread.start()

    def keyPressEvent(self, event):
        if event.key() == QtCore.Qt.Key_Q:
            print("Killing")
            self.deleteLater()
        elif event.key() == QtCore.Qt.Key_Enter:
            self.proceed()
        event.accept()

    def proceed(self):
        print("call enter key")

    def clicked(self, item):

        path1 = "C:\Personal\dataset\Washinghands\\" + item.text()

        self.thread._run_flag = False
        self.thread.path = path1
        self.thread.change_pixmap_signal.connect(self.update_image)

        # start the thread
        #self.thread.start()
        self.thread._run_flag = False
        self.thread = None

        self.thread = VideoThread()
        self.thread.change_pixmap_signal.connect(self.update_image)
        self.thread.path = path1
        # start the thread
        self.thread.start()




    def closeEvent(self, event):
        self.thread.stop()
        event.accept()

    @pyqtSlot(np.ndarray)
    def update_image(self, cv_img):
        """Updates the image_label with a new opencv image"""
        qt_img = self.convert_cv_qt(cv_img)
        self.image_label.setPixmap(qt_img)

    def convert_cv_qt(self, cv_img):
        """Convert from an opencv image to QPixmap"""
        rgb_image = cv2.cvtColor(cv_img, cv2.COLOR_BGR2RGB)
        h, w, ch = rgb_image.shape
        bytes_per_line = ch * w
        convert_to_Qt_format = QtGui.QImage(rgb_image.data, w, h, bytes_per_line, QtGui.QImage.Format_RGB888)
        p = convert_to_Qt_format.scaled(self.disply_width, self.display_height, Qt.KeepAspectRatio)
        return QPixmap.fromImage(p)


if __name__ == "__main__":
    app = QApplication(sys.argv)
    a = App()
    a.show()
    sys.exit(app.exec_())
