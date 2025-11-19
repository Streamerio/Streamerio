function createJson(sheetName, initRowIndex, prop) {
    var jsonData = {};

    for (const p in prop) {
        jsonData[p] = getRows(sheetName.PLAYER_STATUS, initRowIndex, prop[p].column);
    }

    return jsonData;
}