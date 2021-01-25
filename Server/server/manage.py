import os
import sys
import django
from django.conf import settings

print(f"Using Django {django.get_version()}")

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

INSTALLED_APPS = [
    'server.entities'   # remove server. when using manage.py and add it when running server
]

DATABASES = {
    'default': {
        'ENGINE': 'django.db.backends.sqlite3',
        'NAME': BASE_DIR + '/entities/data.db'
    }
}

settings.configure(
    INSTALLED_APPS=INSTALLED_APPS,
    DATABASES=DATABASES
)

django.setup()

if __name__ == "__main__":
    from django.core.management import execute_from_command_line
    execute_from_command_line(sys.argv)
