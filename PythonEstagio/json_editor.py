import sys
import json
import ast
from PyQt5 import QtWidgets, QtGui, QtCore
from PyQt5.QtWidgets import QStyledItemDelegate

# === Custom labels ===
detailsLeft = [
    "Area Bruta Privativa:",
    "Area Total do Lote:",
    "Quartos:",
    "Piso:",
    "Elevador:",
    "Carr. Carros Eletricos:"
]
detailsRight = [
    "Area Bruta:",
    "Area Util:",
    "Ano de Construção:",
    "Casas de Banho:",
    "Estacionamento:",
    "Eficiência Energética:"
]
# ========================

# Light and Dark style sheets
light_qss = """
/* Base font & off-white background */
* { font-family: 'Roboto', sans-serif; }
QMainWindow { background-color: #F9F9F9; }

/* Toolbar flat, ghost buttons */
QToolBar { background-color: #FFFFFF; spacing: 8px; padding: 4px; min-height: 36px; border-bottom: 1px solid #DDD; }
QToolBar QToolButton, QToolBar QLineEdit, QToolBar QPushButton { font-size: 15px; font-weight: 400; }
QToolBar QToolButton { background: transparent; }
QToolBar QToolButton:hover { background-color: rgba(0,150,136,0.1); }

/* Accent color */
:selection, QTreeView::item:selected { background-color: #009688; color: #FFFFFF; }

/* Tree view typography, spacing & separators */
QTreeView {
  background-color: #FFFFFF;
  alternate-background-color: #F9F9F9;
  font-size: 15px;
  font-weight: 300;
  show-decoration-selected: 1;
}
QTreeView::item {
  padding: 6px 8px;
  border-bottom: 1px solid #E0E0E0;
}
QTreeView::item:hover {
  background-color: rgba(0, 0, 0, 0.05);
}
/* — Header Row (Light) — */
QHeaderView {
  background: transparent;
}
QHeaderView::section {
  background-color: #E0E0E0;
  padding: 6px 12px;
  border: none;
  font-size: 15px;
  font-weight: 500;
  /* round only the top corners of the entire header */
  border-top-left-radius: 8px;
  border-top-right-radius: 8px;
}


/* Inputs & buttons */
QLineEdit, QToolButton, QGroupBox, QPushButton { background-color: #FFFFFF; color: #000000; font-size: 14px; font-weight: 400; }
QLineEdit { border: 1px solid #CCC; border-radius: 4px; padding: 4px; }

/* — Light Mode: scrollbar handle ≈ #F0F0F0 darkened 10% (via rgba) — */
QScrollBar:vertical, QScrollBar:horizontal {
  background: rgba(10,10,10,0.10);
  width: 12px; height: 12px; margin: 0;
}
QScrollBar::add-line, QScrollBar::sub-line {
  width: 0; height: 0;
}
QScrollBar::add-page, QScrollBar::sub-page {
  background: transparent;
}
QScrollBar::handle:vertical, QScrollBar::handle:horizontal {
  /* black @ 10% against #F0F0F0 → ~#E5E5E5 */
  background: rgba(0,0,0,0.10);
  min-height: 20px; min-width: 20px; border-radius: 6px;
}
QScrollBar::handle:hover {
  /* black @ 15% → ~#DBDBDB */
  background: rgba(0,0,0,0.15);
}
QScrollBar::handle:pressed {
  /* black @ 20% → ~#D9D9D9 */
  background: rgba(0,0,0,0.20);
}
QScrollBar::groove {
  background: transparent; margin: 0; border-radius: 6px;
}


"""

dark_qss = """
/* Base font & dark gray background */
* { font-family: 'Roboto', sans-serif; }
QMainWindow { background-color: #212121; }

/* Toolbar flat, ghost buttons */
QToolBar { background-color: #2B2B2B; spacing: 8px; padding: 4px; min-height: 36px; border-bottom: 1px solid #444; }
QToolBar QToolButton, QToolBar QLineEdit, QToolBar QPushButton { font-size: 15px; font-weight: 400; color: #FFFFFF; }
QToolBar QToolButton { background: transparent; }
QToolBar QToolButton:hover { background-color: rgba(77,182,172,0.1); }

/* Accent color */
:selection, QTreeView::item:selected { background-color: #4DB6AC; color: #000000; }

/* Tree view typography, spacing & separators */
QTreeView {
  background-color: #2C2C2C;
  alternate-background-color: #212121;
  color: #FFFFFF;
  font-size: 15px;
  font-weight: 300;
  show-decoration-selected: 1;
}
QTreeView::item {
  padding: 6px 8px;
  border-bottom: 1px solid #383838;
}
QTreeView::item:hover {
  background-color: rgba(255, 255, 255, 0.05);
}
/* — Header Row (Dark) — */
QHeaderView {
  background: transparent;
}
QHeaderView::section {
  background-color: #383838;
  color: #FFFFFF;
  padding: 6px 12px;
  border: none;
  font-size: 15px;
  font-weight: 500;
  border-top-left-radius: 8px;
  border-top-right-radius: 8px;
}


/* Inputs & buttons */
QLineEdit, QToolButton, QGroupBox, QPushButton { background-color: #2C2C2C; color: #FFFFFF; font-size: 14px; font-weight: 400; }
QLineEdit { border: 1px solid #555; border-radius: 4px; padding: 4px; }

/* — Dark Mode: scrollbar handle ≈ #2C2C2C lightened 10% (via rgba) — */
QScrollBar:vertical, QScrollBar:horizontal {
  background: rgba(225,225,225,0.10);
  width: 12px; height: 12px; margin: 0;
}
QScrollBar::add-line, QScrollBar::sub-line {
  width: 0; height: 0;
}
QScrollBar::add-page, QScrollBar::sub-page {
  background: transparent;
}
QScrollBar::handle:vertical, QScrollBar::handle:horizontal {
  /* white @ 10% against #2C2C2C → ~#393939 */
  background: rgba(255,255,255,0.10);
  min-height: 20px; min-width: 20px; border-radius: 6px;
}
QScrollBar::handle:hover {
  /* white @ 15% → ~#404040 */
  background: rgba(255,255,255,0.15);
}
QScrollBar::handle:pressed {
  /* white @ 20% → ~#4C4C4C */
  background: rgba(255,255,255,0.20);
}
QScrollBar::groove {
  background: transparent; margin: 0; border-radius: 6px;
}


"""

class DropArea(QtWidgets.QWidget):
    fileDropped = QtCore.pyqtSignal(str)

    def __init__(self, parent=None):
        super().__init__(parent)
        self.setAcceptDrops(True)
        layout = QtWidgets.QVBoxLayout(self)
        self.label = QtWidgets.QLabel(
            "\n\n Drop a JSON file here \n\n or click to browse",
            alignment=QtCore.Qt.AlignCenter
        )
        self.label.setStyleSheet("border: 2px dashed #aaa; font-size: 16px; padding: 40px;")
        layout.addWidget(self.label)
        self.setLayout(layout)

    def dragEnterEvent(self, event):
        if event.mimeData().hasUrls():
            event.accept()
        else:
            event.ignore()

    def dropEvent(self, event):
        for url in event.mimeData().urls():
            path = url.toLocalFile()
            if path.lower().endswith('.json'):
                self.fileDropped.emit(path)
                break

    def mousePressEvent(self, event):
        path, _ = QtWidgets.QFileDialog.getOpenFileName(
            self, 'Open JSON', '', 'JSON Files (*.json)'
        )
        if path:
            self.fileDropped.emit(path)

class JsonModel(QtGui.QStandardItemModel):
    def __init__(self, data=None, parent=None):
        super().__init__(parent)
        self.setHorizontalHeaderLabels(['Key', 'Value'])
        if data:
            self.loadData(data)

    def loadData(self, data, parent=None):
        parent = parent or self.invisibleRootItem()
        if isinstance(data, dict):
            for key, value in data.items():
                if key in ("detailsLeft", "detailsRight"):
                    vals = value.splitlines() if isinstance(value, str) else value
                    labels = detailsLeft if key == "detailsLeft" else detailsRight
                    for i, v in enumerate(vals):
                        ki = QtGui.QStandardItem(labels[i] if i < len(labels) else f"[{i}]")
                        ki.setEditable(False)
                        vi = QtGui.QStandardItem(str(v))
                        vi.setEditable(True)
                        parent.appendRow([ki, vi])
                    continue
                ki = QtGui.QStandardItem(str(key))
                ki.setEditable(False)
                if isinstance(value, (dict, list)):
                    parent.appendRow([ki, QtGui.QStandardItem('')])
                    self.loadData(value, ki)
                else:
                    vi = QtGui.QStandardItem(str(value))
                    vi.setEditable(True)
                    parent.appendRow([ki, vi])
        elif isinstance(data, list):
            for i, item in enumerate(data):
                ki = QtGui.QStandardItem(f"[{i}]")
                ki.setEditable(False)
                if isinstance(item, (dict, list)):
                    parent.appendRow([ki, QtGui.QStandardItem('')])
                    self.loadData(item, ki)
                else:
                    vi = QtGui.QStandardItem(str(item))
                    vi.setEditable(True)
                    parent.appendRow([ki, vi])

    def toData(self, parent=None):
        def parseItem(item):
            if item.hasChildren():
                keys = [item.child(r, 0).text() for r in range(item.rowCount())]
                if not all(k.startswith('[') for k in keys):
                    d, left, right = {}, {}, {}
                    for r in range(item.rowCount()):
                        k = item.child(r, 0).text()
                        v = parseValue(item.child(r, 1).text())
                        if k in detailsLeft:
                            left[k] = v
                        elif k in detailsRight:
                            right[k] = v
                        else:
                            ch = item.child(r, 0)
                            d[k] = parseItem(ch) if ch.hasChildren() else v
                    if left:
                        d['detailsLeft'] = '\n'.join(str(left[l]) for l in detailsLeft if l in left)
                    if right:
                        d['detailsRight'] = '\n'.join(str(right[r]) for r in detailsRight if r in right)
                    return d
                return [
                    parseItem(item.child(r, 0)) if item.child(r, 0).hasChildren()
                    else parseValue(item.child(r, 1).text())
                    for r in range(item.rowCount())
                ]
            return parseValue(item.text())
        return parseItem(parent or self.invisibleRootItem())


def parseValue(s):
    try:
        return ast.literal_eval(s)
    except:
        return s

class TreeFilterProxyModel(QtCore.QSortFilterProxyModel):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.filterString = ''
        self.searchInKeys = False
        self.showWholeCard = True

    def setFilterString(self, text):
        self.filterString = text.lower()
        self.invalidateFilter()

    def setSearchInKeys(self, flag):
        self.searchInKeys = flag
        self.invalidateFilter()

    def setShowWholeCard(self, flag):
        self.showWholeCard = flag
        self.invalidateFilter()

    def hasDescendantMatch(self, index):
        model = self.sourceModel()
        for row in range(model.rowCount(index)):
            col = 0 if self.searchInKeys else 1
            idx = model.index(row, col, index)
            if self.filterString in str(model.data(idx) or '').lower():
                return True
            if self.hasDescendantMatch(model.index(row, 0, index)):  # recurse
                return True
        return False

    def filterAcceptsRow(self, sourceRow, sourceParent):
        if not self.filterString:
            return True
        model = self.sourceModel()
        idx = model.index(sourceRow, 0 if self.searchInKeys else 1, sourceParent)
        if self.filterString in str(model.data(idx) or '').lower():
            return True
        idx0 = model.index(sourceRow, 0, sourceParent)
        if self.hasDescendantMatch(idx0):
            return True
        if self.showWholeCard and sourceParent.isValid():
            if self.hasDescendantMatch(sourceParent):
                return True
        return False
    
class PlusButtonDelegate(QStyledItemDelegate):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.parent = parent
        self.hitWidth = 20  # px for clickable area

    def paint(self, painter, option, index):
        super().paint(painter, option, index)
        text = index.data()
        if index.column() == 0 and text in ("sections", "cards"):
            # draw a tiny “+”
            r = option.rect
            size = 8
            x = r.x()
            y = r.y() + (r.height() - size)//2
            pen = painter.pen()
            painter.setPen(QtGui.QPen(QtGui.QColor(0,150,136), 2))
            painter.drawLine(x, y + size//2, x + size, y + size//2)
            painter.drawLine(x + size//2, y, x + size//2, y + size)
            painter.setPen(pen)

    def editorEvent(self, event, model, option, index):
        if (index.column() == 0 and
            index.data() in ("sections", "cards") and
            event.type() == QtCore.QEvent.MouseButtonRelease):
            relx = event.pos().x() - option.rect.x()
            if relx < self.hitWidth:
                if index.data() == "sections":
                    self.parent.addSection()
                else:
                    self.parent.addCard(index)
                return True
        return super().editorEvent(event, model, option, index)

class JsonEditor(QtWidgets.QMainWindow):
    def __init__(self):
        super().__init__()
        self.current_theme = 'light'
        self.init_ui()
        self.apply_theme()

    def init_ui(self):
        self.setWindowTitle('JSON Catalog Editor')
        self.resize(900, 650)

        # Toolbar
        tb = self.addToolBar('Main')
        tb.setMovable(False)
        icon_open = self.style().standardIcon(QtWidgets.QStyle.SP_DialogOpenButton)
        icon_save = self.style().standardIcon(QtWidgets.QStyle.SP_DialogSaveButton)

        # Open & Save
        a_open = QtWidgets.QAction(icon_open, 'Open', self)
        a_open.triggered.connect(self.openFile)
        tb.addAction(a_open)
        a_save = QtWidgets.QAction(icon_save, 'Save', self)
        a_save.triggered.connect(self.saveFile)
        tb.addAction(a_save)

        tb.addSeparator()
        # Collapse & Expand
        a_collapse = QtWidgets.QAction('Collapse All', self)
        a_collapse.triggered.connect(lambda: self.tree.collapseAll())
        tb.addAction(a_collapse)
        a_expand = QtWidgets.QAction('Expand All', self)
        a_expand.triggered.connect(lambda: self.tree.expandAll())
        tb.addAction(a_expand)

        tb.addSeparator()
        # Search input
        self.search_input = QtWidgets.QLineEdit()
        self.search_input.setPlaceholderText('Search...')
        tb.addWidget(self.search_input)

        # Options dropdown
        btn_opts = QtWidgets.QToolButton(self)
        btn_opts.setText('Options')
        btn_opts.setPopupMode(QtWidgets.QToolButton.InstantPopup)
        menu = QtWidgets.QMenu(btn_opts)
        w = QtWidgets.QWidget()
        ly = QtWidgets.QVBoxLayout(w)
        g1 = QtWidgets.QGroupBox('Search In')
        h1 = QtWidgets.QHBoxLayout(g1)
        rbV = QtWidgets.QRadioButton('Values')
        rbK = QtWidgets.QRadioButton('Keys')
        rbV.setChecked(True)
        h1.addWidget(rbV)
        h1.addWidget(rbK)
        ly.addWidget(g1)
        g2 = QtWidgets.QGroupBox('Show')
        h2 = QtWidgets.QHBoxLayout(g2)
        rbLine = QtWidgets.QRadioButton('Line Only')
        rbCard = QtWidgets.QRadioButton('Whole Card')
        rbCard.setChecked(True)
        h2.addWidget(rbLine)
        h2.addWidget(rbCard)
        ly.addWidget(g2)
        actW = QtWidgets.QWidgetAction(menu)
        actW.setDefaultWidget(w)
        menu.addAction(actW)
        btn_opts.setMenu(menu)
        tb.addWidget(btn_opts)

        tb.addSeparator()
        # Theme toggles
        theme_act = QtWidgets.QAction('Toggle Theme', self)
        theme_act.triggered.connect(self.toggle_theme)
        tb.addAction(theme_act)

        # Central
        self.proxy = TreeFilterProxyModel(self)
        self.stack = QtWidgets.QStackedWidget()
        self.setCentralWidget(self.stack)
        self.drop = DropArea()
        self.stack.addWidget(self.drop)
        page = QtWidgets.QWidget()
        v = QtWidgets.QVBoxLayout(page)
        self.tree = QtWidgets.QTreeView()
        v.addWidget(self.tree)
        self.stack.addWidget(page)

        # persistent line‐edit under the tree for the “Value” column
        self.bottom_edit = QtWidgets.QLineEdit(self)
        self.bottom_edit.setPlaceholderText("Edit selected value …")
        self.bottom_edit.setMinimumHeight(24)
        v.addWidget(self.bottom_edit)


        # Signals
        self.drop.fileDropped.connect(self.loadJson)
        self.search_input.textChanged.connect(self.proxy.setFilterString)
        rbK.toggled.connect(lambda f: self.proxy.setSearchInKeys(f))
        rbV.toggled.connect(lambda f: self.proxy.setSearchInKeys(not f))
        rbLine.toggled.connect(lambda f: self.proxy.setShowWholeCard(not f))
        rbCard.toggled.connect(lambda f: self.proxy.setShowWholeCard(f))

        self.current_path = None

    def apply_theme(self):
        self.setStyleSheet(light_qss if self.current_theme == 'light' else dark_qss)

    def toggle_theme(self):
        self.current_theme = 'dark' if self.current_theme == 'light' else 'light'
        self.apply_theme()

    def openFile(self):
        path, _ = QtWidgets.QFileDialog.getOpenFileName(self, 'Open JSON', '', 'JSON Files (*.json)')
        if path:
            self.loadJson(path)

    def loadJson(self, path):
        try:
            with open(path, 'r', encoding='utf-8') as f:
                data = json.load(f)
        except Exception as e:
            QtWidgets.QMessageBox.warning(self, 'Error', f'Failed to load JSON:\n{e}')
            return
        model = JsonModel(data)
        self.proxy.setSourceModel(model)
        self.tree.setModel(self.proxy)
        hdr = self.tree.header()
        hdr.setSectionResizeMode(0, QtWidgets.QHeaderView.Interactive)
        hdr.setSectionResizeMode(1, QtWidgets.QHeaderView.Stretch)
        self.tree.setColumnWidth(0, self.width() // 3)
        self.tree.setContextMenuPolicy(QtCore.Qt.CustomContextMenu)
        self.tree.customContextMenuRequested.connect(self.openMenu)
        self.tree.expandAll()
        self.search_input.clear()
        self.stack.setCurrentIndex(1)
        self.current_path = path

                # ────────────────────────────────────────────────────────────────────
        # Install our “+”‐button delegate on the Key column
        self.delegate = PlusButtonDelegate(self)
        self.tree.setItemDelegateForColumn(0, self.delegate)

        self.current_path = path

    # install plus‐button decorator on column 0
        self.delegate = PlusButtonDelegate(self)
        self.tree.setItemDelegateForColumn(0, self.delegate)

    # ─── hook up bottom‐bar editing now that the model exists ────────────
        sel = self.tree.selectionModel()
        sel.currentChanged.connect(self.onSelectionChanged)
        self.bottom_edit.textChanged.connect(self.onBottomEdited)



    def saveFile(self):
        if not self.current_path:
            path, _ = QtWidgets.QFileDialog.getSaveFileName(self, 'Save JSON', '', 'JSON Files (*.json)')
            if not path:
                return
            self.current_path = path
        data = self.proxy.sourceModel().toData()
        try:
            with open(self.current_path, 'w', encoding='utf-8') as f:
                json.dump(data, f, indent=2, ensure_ascii=False)
            QtWidgets.QMessageBox.information(self, 'Saved', f'Saved to {self.current_path}')
        except Exception as e:
            QtWidgets.QMessageBox.warning(self, 'Error', f'Failed to save JSON:\n{e}')

    def openMenu(self, pos):
        idx = self.tree.indexAt(pos)
        if not idx.isValid():
            return
        menu = QtWidgets.QMenu()
        action = menu.addAction('Delete')
        if menu.exec_(self.tree.viewport().mapToGlobal(pos)) == action:
            if QtWidgets.QMessageBox.question(
                self, 'Confirm Delete', 'Delete this item?',
                QtWidgets.QMessageBox.Yes | QtWidgets.QMessageBox.No
            ) != QtWidgets.QMessageBox.Yes:
                return

            # map the proxy index back to the source model
            src_idx    = self.proxy.mapToSource(idx)
            src_parent = src_idx.parent()
            src_row    = src_idx.row()

            # remove that row from the source model
            self.proxy.sourceModel().removeRow(src_row, src_parent)

    def resizeEvent(self, event):
        super().resizeEvent(event)
        if hasattr(self, 'tree'):
            self.tree.setColumnWidth(0, self.width() // 3)

    def cloneRow(self, keyItem):
        
        model = keyItem.model()
        parent = keyItem.parent() or model.invisibleRootItem()
        row = keyItem.row()
        
        origVal = model.item(row, 1)
        
        cloneKey   = QtGui.QStandardItem(keyItem.text())
        cloneKey.setEditable(keyItem.isEditable())
        cloneKey.setIcon(keyItem.icon())
        if origVal is not None:
            cloneVal = QtGui.QStandardItem(origVal.text())
            cloneVal.setEditable(origVal.isEditable())
            cloneVal.setIcon(origVal.icon())
        else:
            cloneVal = QtGui.QStandardItem()
        
        for i in range(keyItem.rowCount()):
            childKey = keyItem.child(i, 0)
            ck, cv = self.cloneRow(childKey)
            cloneKey.appendRow([ck, cv])
        
        return cloneKey, cloneVal
    
    def addSection(self):
        m = self.proxy.sourceModel()
        root = m.invisibleRootItem()
        sectionsItem = root.child(0, 0)
        if sectionsItem.text() != "sections" or sectionsItem.rowCount() == 0:
            return
        firstSec = sectionsItem.child(0, 0)
        newKey, newVal = self.cloneRow(firstSec)
        # rename the “[0]” to the new index
        newKey.setText(f"[{sectionsItem.rowCount()}]")
        sectionsItem.appendRow([newKey, newVal])
        # ensure it’s shown
        secIdx = sectionsItem.index()
        self.tree.expand(self.proxy.mapFromSource(secIdx))

    def addCard(self, proxyIndex):
        # proxyIndex is the QModelIndex for the “cards” cell
        src = self.proxy.mapToSource(proxyIndex)
        m = self.proxy.sourceModel()
        cardsItem = m.itemFromIndex(src)
        if cardsItem.rowCount() == 0:
            return
        firstCard = cardsItem.child(0, 0)
        newKey, newVal = self.cloneRow(firstCard)
        newKey.setText(f"[{cardsItem.rowCount()}]")
        cardsItem.appendRow([newKey, newVal])
        # expand that section’s “cards”
        self.tree.expand(proxyIndex)

    def onSelectionChanged(self, current: QtCore.QModelIndex, previous: QtCore.QModelIndex):
        # map proxy → source, then grab the VALUE column
        src_idx = self.proxy.mapToSource(current)
        val_idx = src_idx.sibling(src_idx.row(), 1)
        item    = self.proxy.sourceModel().itemFromIndex(val_idx)
        self.bottom_edit.blockSignals(True)
        self.bottom_edit.setText(item.text() if item else "")
        self.bottom_edit.blockSignals(False)

    def onBottomEdited(self, text: str):
        idx = self.tree.currentIndex()
        if not idx.isValid():
            return
        src_idx = self.proxy.mapToSource(idx)
        val_idx = src_idx.sibling(src_idx.row(), 1)
        item    = self.proxy.sourceModel().itemFromIndex(val_idx)
        if item:
            item.setText(text)



if __name__ == '__main__':
    app = QtWidgets.QApplication(sys.argv)
    editor = JsonEditor()
    editor.show()
    sys.exit(app.exec_())
