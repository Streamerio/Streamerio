function getEnvParam(paramName) {
    return PropertiesService.getScriptProperties().getProperty(paramName);
}

function createOutput(jsonData) {
  const output = ContentService.createTextOutput(JSON.stringify(jsonData));
  output.setMimeType(ContentService.MimeType.JSON);

  return output;
}