function doGet(e) {
    const dataType = e.parameter.dataType;

    var jsonData;
    switch(dataType) {
        case sheetName.GAME_SETTINGS:
            jsonData = createJson(dataType, gameSettingsInitRowIndex, gameSettingsProp);
            break;
        case sheetName.PLAYER_STATUS:
            jsonData = createJson(dataType, playerStatusInitRowIndex, playerStatusProp);
            break;
        case sheetName.ENEMY_STATUS:
            jsonData = createJson(dataType, enemyStatusInitRowIndex, enemyStatusProp);
            break;
        case sheetName.ULT_STATUS:
            jsonData = createJson(dataType, ultStatusInitRowIndex, ultStatusProp);
            break;
    }

    console.log(jsonData);

    return createOutput(jsonData);
}