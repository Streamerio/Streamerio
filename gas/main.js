function doGet(e) {
    const dataType = e.parameter.dataType;

    var jsonData;
    switch(dataType) {
        case sheetName.GAME_SETTINGS:
            jsonData = createJson(sheetName, gameSettingsInitRowIndex, gameSettingsProp);
            break;
        case sheetName.PLAYER_STATUS:
            jsonData = createJson(sheetName, playerStatusInitRowIndex, playerStatusProp);
            break;
        case sheetName.ENEMY_STATUS:
            jsonData = createJson(sheetName, enemyStatusInitRowIndex, enemyStatusProp);
            break;
        case sheetName.ULT_STATUS:
            jsonData = createJson(sheetName, ultStatusInitRowIndex, ultStatusProp);
            break;
    }

    console.log(jsonData);

    return createOutput(jsonData);
}