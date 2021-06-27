require.config({ paths: { vs: '/lib/monaco' } });

/**
 * Creates the Monaco editor and sets it up accordingly
 * 
 * @param {{
 *	editorDiv?: string | HTMLDivElement,
 *	form?: string | HTMLFormElement,
 *	input?: string | HTMLInputElement,
 *	readonly?: boolean,
 *	languageSelector?: string | HTMLSelectElement,
 *	initialValue?: string
 * }} options
 */
function setupEditor(options) {
	var editorDiv = (typeof options.editorDiv === 'string'
		? document.getElementById(options.editorDiv)
		: options.editorDiv) || document.getElementById('editor');
	var form = typeof options.form === 'string'
		? document.getElementById(options.form)
		: options.form;
	var input = typeof options.input === 'string'
		? document.getElementById(options.input)
		: options.input;
	var readonly = !!options.readonly;
	var languageSelector = (typeof options.languageSelector === 'string'
		? document.getElementById(options.languageSelector)
		: options.languageSelector) || document.getElementById('script-language-selector');
	var initialValue = options.initialValue || '';

	var languages = [
		'python',
		'lua',
	];
	var editor;

	require(['vs/editor/editor.main'], function () {
		editor = monaco.editor.create(editorDiv, {
			value: initialValue,
			language: 'powershell',
			minimap: false,
			scrollBeyondLastLine: false,
			heightInPx: 400,
			readonly: readonly
		});

		if (form && input) {
			form.onsubmit = function () {
				input.value = monaco.editor
					.getModel('inmemory://model/1')
					.getValue()
			}
		}
	});

	if (languageSelector) {
		languageSelector.onchange = function () {
			var model = editor.getModel('inmemory://model/1');

			monaco.editor.setModelLanguage(model, languages[parseInt(languageSelector.value, 10)]);
		}
	}
};