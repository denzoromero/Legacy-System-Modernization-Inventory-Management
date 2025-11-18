

async function fetchJson(url, options = {}) {
    // Set default headers for JSON
    const defaultOptions = {
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            ...options.headers // Merge with custom headers
        },
        ...options // Spread other options
    };

    try {
        const response = await fetch(url, defaultOptions);

        const status = response.status;
        const contentType = response.headers.get('content-type');
        const isJson = contentType && contentType.includes('application/json');

        if (!response.ok) {

            let errorMessage = `HTTP error ${status}`;

            try {
                if (isJson) {
                    const errorData = await response.json();
                    errorMessage = errorData.message || errorData.toString();
                } else {
                    errorMessage = await response.text() || errorMessage;
                }
            } catch (parseError) {
                console.warn('Could not parse error response:', parseError);
            }

            const enhancedError = new Error(errorMessage);
            enhancedError.status = response.status;
            enhancedError.url = url;      

            throw enhancedError;
      
        }

        //JSON
        //if (isJson) {
        //}
        return await response.json();

        //TEXT
        /*  return await response.text();*/

    } catch (error) {

        console.error('Fetch error:', {
            url,
            error: error.message,
            status: error.status,
            timestamp: new Date().toISOString()
        });

        /*console.error('Fetch error:', {url, error: error.message, status:error.status, timestamp: new Date().toISOString()});*/

        if (error.name === 'TypeError' && error.message.includes('Failed to fetch')) {
            throw new Error('Network error: Unable to connect to server');
        } else if (error.name === 'SyntaxError') {
            throw new Error('Invalid JSON response from server');
        }

        throw error;
    }
}


const handleAuthenticationError = (error) => {

    appendAlertWithoutAnimation(error.message, 'danger');

    const encodedMessage = encodeURIComponent(error.message);

    setTimeout(function () { window.location.href = `/Home/Ferramentaria?message=${encodedMessage}`; }, 1000);

    /*window.location.href = `/Account/Login?message=${encodedMessage}&returnUrl=${encodeURIComponent(window.location.pathname)}`;*/

}


class ValidationError extends Error {
    constructor(message) {
        super(message);
        this.name = 'ValidationError';
        this.status = 400;
    }
}