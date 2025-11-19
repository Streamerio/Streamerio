const spreadSheet = SpreadsheetApp.openById(getEnvParam('SPREAD_SHEET_ID'));

const sheetName = {
    GAME_SETTINGS: 'GameSettings',
    PLAYER_STATUS: 'PlayerStatus',
    ULT_STATUS: 'UltStatus',
    ENEMY_STATUS: 'EnemyStatus',
}

function getSheet(sheetName) {
    return spreadSheet.getSheetByName(sheetName);
}

function getRows(sheetName, initRowIndex, columnIndex) {
    const sheet = getSheet(sheetName);
    const lastRow = sheet.getLastRow();
    if (initRowIndex > lastRow) {
        console.warn(`Requested row index ${initRowIndex} exceeds last row ${lastRow} in sheet ${sheetName}.`);
        return [];
    }

    const values2D = sheet.getRange(initRowIndex, columnIndex, lastRow-(initRowIndex-1), 1).getValues();
    return values2D.flat();
}