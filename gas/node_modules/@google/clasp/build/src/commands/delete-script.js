import { Command } from 'commander';
import inquirer from 'inquirer';
import { intl } from '../intl.js';
import { isInteractive, withSpinner } from './utils.js';
export const command = new Command('delete-script')
    .alias('delete')
    .description('Delete a project')
    .argument('[scriptId]', 'Apps Script ID to list deployments for')
    .option('-f, --force', "Bypass any confirmation messages. It's not a good idea to do this unless you want to run clasp from a script.")
    .action(async function (scriptId) {
    const options = this.optsWithGlobals();
    const clasp = options.clasp;
    if (scriptId) {
        clasp.withScriptId(scriptId);
    }
    if (!clasp.project.scriptId) {
        const msg = intl.formatMessage({ id: "6dAsTC", defaultMessage: [{ type: 0, value: "Script ID not set, unable to delete the script." }] });
        this.error(msg);
    }
    //ask confirmation
    let confirmed = options.force || false;
    if (!confirmed && isInteractive()) {
        const promptDeleteDriveFiles = intl.formatMessage({ id: "DKO5+J", defaultMessage: [{ type: 0, value: "Are you sure you want to delete the script?" }] });
        const answerDeleteDriveFiles = await inquirer.prompt([
            {
                default: false,
                message: promptDeleteDriveFiles,
                name: 'answer',
                type: 'confirm',
            },
        ]);
        confirmed = answerDeleteDriveFiles.answer;
    }
    if (!confirmed) {
        return;
    }
    const spinnerMsg = intl.formatMessage({ id: "lRbOjS", defaultMessage: [{ type: 0, value: "Deleting your scripts..." }] });
    await withSpinner(spinnerMsg, async () => await clasp.project.trashScript());
    const successMessage = intl.formatMessage({ id: "zDXU5q", defaultMessage: [{ type: 0, value: "Deleted script " }, { type: 1, value: "scriptId" }] }, { scriptId: clasp.project.scriptId });
    console.log(successMessage);
});
